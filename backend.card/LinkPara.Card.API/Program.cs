using Elastic.Apm.NetCoreAll;
using FluentValidation.AspNetCore;
using LinkPara.Authentication;
using LinkPara.Card.Application;
using LinkPara.Card.Application.Commons.Models.ElasticApmConfiguration;
using LinkPara.Card.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Card.Infrastructure;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using LinkPara.SharedModels.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.RabbitMQ;
using System.Reflection;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.Card.API.Swagger;
using Newtonsoft.Json.Converters;
using Microsoft.EntityFrameworkCore;
using LinkPara.Card.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilterAttribute>())
    .AddFluentValidation(x => x.AutomaticValidationEnabled = false)
    .AddNewtonsoftJson(options =>
    { 
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((options) =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Card API", Version = "v1" });
    options.SchemaFilter<EnumDescriptionSchemaFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "You should use token info that can obtained from account-login controller.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSwaggerGenNewtonsoftSupport();
var vaultClient = builder.Services.AddVault(configuration);
builder.Services.AddApplication(configuration);
await builder.Services.AddApplicationUser(vaultClient);
builder.Services.AddInfrastructure(configuration, vaultClient);
builder.Services.AddJwtAuthorization(vaultClient);

var eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Logger(new LoggerConfiguration()
        .MinimumLevel.Error()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("ApplicationName", "Card.API")
        .WriteTo.Debug()
        .WriteTo.RabbitMQ(
        (clientConfiguration, sinkConfiguration) =>
        {
            sinkConfiguration.BatchPostingLimit = 100;
            sinkConfiguration.EmitEventFailure = EmitEventFailureHandling.WriteToFailureSink;
            sinkConfiguration.RestrictedToMinimumLevel = LogEventLevel.Error;
            sinkConfiguration.BufferingTimeLimit = TimeSpan.FromSeconds(2);
            sinkConfiguration.TextFormatter = new ExceptionLogFormatter();

            clientConfiguration.AutoCreateExchange = true;
            clientConfiguration.DeliveryMode = RabbitMQDeliveryMode.Durable;
            clientConfiguration.Exchange = "Log.ExceptionLog";
            clientConfiguration.ExchangeType = "fanout";
            clientConfiguration.Hostnames = new[] { eventBusSettings.Host };
            clientConfiguration.Username = eventBusSettings.Username;
            clientConfiguration.Password = eventBusSettings.Password;
        },
        failureSinkConfiguration =>
        {
            failureSinkConfiguration.Elasticsearch(new ElasticsearchSinkOptions
            (new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "SerilogSettings", "NodeUri")))
            {
                IndexFormat = "exception-logs-{0:yyyy.MM}",
                AutoRegisterTemplate = true
            });
        }
        )
    .CreateLogger());
});

builder.Services.ConfigureLocalization(); // Required for localization

var app = builder.Build();

app.ConfigureLocalization(); // Required for localization

using (var scope = app.Services.CreateScope())
{
    try
    {
        var enableAutoMigrate = configuration.GetValue<bool>("Database:EnableAutoMigrate", false);
        if (enableAutoMigrate)
        {
            var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
            var startupLogger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
            await DatabaseInitializer.EnsureMigrationBaselineAsync(scope.ServiceProvider, startupLogger);
            db.Database.Migrate();
            try
            {
                await DatabaseInitializer.EnsureTablesExistAsync(scope.ServiceProvider, configuration, startupLogger);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while ensuring tables exist after migration.");
                throw;
            }
        }
        else
        {
            Log.Information("Automatic database migration is disabled via configuration (Database:EnableAutoMigrate = false). Skipping migrations at startup.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while attempting to migrate or initialize the database.");
        throw;
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "Card.API" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

// Health Check
app.MapGet("/HealthCheck", () => "Healthy").WithName("HealthCheckController");

app.Run();
