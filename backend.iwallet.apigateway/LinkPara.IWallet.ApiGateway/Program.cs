using Elastic.Apm.Api;
using Elastic.Apm.NetCoreAll;
using LinkPara.IWallet.ApiGateway.Filters;
using LinkPara.IWallet.ApiGateway.Models;
using LinkPara.IWallet.ApiGateway.Services;
using LinkPara.HttpProviders.Vault;
using LinkPara.RateLimit;
using LinkPara.Security;
using LinkPara.SharedModels.Localization;
using MassTransit;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;
using LinkPara.IWallet.ApiGateway.Authorization;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using LinkPara.IWallet.ApiGateway.Filters.RequestResponseLogging;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using Serilog.Sinks.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddControllers(options =>
        options.Filters.Add<IWalletExceptionFilterAttribute>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "IWallet Gateway API", Version = "v1", });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "You should use token info that can obtained from account-login controller.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var vaultClient = builder.Services.ConfigureVaultHttpClient(configuration);

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
        .Enrich.WithProperty("ApplicationName","LinkPara.IWallet.ApiGateway")
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

builder.Services.AddHttpClient<IVaultClient, VaultClient>(client => (VaultClient)vaultClient);

builder.Services.AddHttpContextAccessor();
builder.Services.RegisterHttpClients(vaultClient);
builder.Services.AddLogging();
builder.Services.AddCors();

builder.Services.AddAuthentication();
builder.Services.AddJwtAuthorization(vaultClient);




builder.Services.AddScoped<IHashGenerator, HashGenerator>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
if (isLocalConfigurationEnabled)
{
    configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
}
else
{
    eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
             h =>
             {
                 h.Username(eventBusSettings.Username);
                 h.Password(eventBusSettings.Password);
             });

    });
});

builder.Services.ConfigureLocalization(); // Required for localization

var app = builder.Build();

app.ConfigureLocalization(); // Required for localization

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x =>
    {
        x.AllowAnyOrigin();
        x.AllowAnyMethod();
        x.AllowAnyHeader();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/HealthCheck", () => "Healthy");

app.UseRequestResponseLoggingMiddleware();

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "IWallet.ApiGateway" }
    });

if (app.Environment.IsDevelopment())
{
    app.UseAllElasticApm(configuration);
}

app.UseGatewayRateLimiter(configuration);

app.Run();