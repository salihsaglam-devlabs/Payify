using Elastic.Apm.NetCoreAll;
using FluentValidation.AspNetCore;
using LinkPara.Authentication;
using LinkPara.Identity.Application;
using LinkPara.Identity.Application.Common.Models.ElasticApmConfiguration;
using LinkPara.Identity.Application.Common.Models.EventBusConfiguration;
using LinkPara.Identity.Infrastructure;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using LinkPara.SharedModels.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.RabbitMQ;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilterAttribute>())
    .AddFluentValidation(x => x.AutomaticValidationEnabled = false)
    .AddNewtonsoftJson();

builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen((options) =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity API", Version = "v1" });
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

});

var vaultClient = builder.Services.AddVault(configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration, vaultClient);
builder.Services.AddIdentityConfiguration(configuration, vaultClient);
builder.Services.AddJwtAuthorization(vaultClient);
builder.Services.AddJwtProviderAuthorization(configuration, vaultClient);
await builder.Services.ConfigureHttpClients(vaultClient);
builder.Services.AddApprovalScreenService();

var eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Logger(new LoggerConfiguration()
        .MinimumLevel.Error()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("ApplicationName", "LinkPara.Identity.API")
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
builder.Services.ConfigureSeedData();

var app = builder.Build();


app.ConfigureLocalization(); // Required for localization


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
        { "ElasticApm:ServiceName", "Identity.API" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

app.MapGet("/HealthCheck", () => "Healthy").WithName("HealthCheckController");

app.Run();