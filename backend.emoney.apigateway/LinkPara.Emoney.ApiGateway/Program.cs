using Elastic.Apm.Api;
using Elastic.Apm.NetCoreAll;
using LinkPara.Emoney.ApiGateway.Authentication;
using LinkPara.Emoney.ApiGateway.Authentication.PrivateKey;
using LinkPara.Emoney.ApiGateway.Filters;
using LinkPara.Emoney.ApiGateway.Models;
using LinkPara.Emoney.ApiGateway.Services;
using LinkPara.HttpProviders.Vault;
using LinkPara.RateLimit;
using LinkPara.Security;
using LinkPara.SharedModels.Localization;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using Serilog.Sinks.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddControllers(options =>
        options.Filters.Add<EmoneyExceptionFilterAttribute>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Emoney Gateway API", Version = "v1", });

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
        .Enrich.WithProperty("ApplicationName", "Emoney.ApiGateway")
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

const string signatureSchemeHandler = "DefaultHandler";

builder.Services.AddAuthentication(options =>
{
    options.AddScheme<PrivateKeySchemeHandler>(signatureSchemeHandler, signatureSchemeHandler);
    options.DefaultChallengeScheme = signatureSchemeHandler;
    options.DefaultForbidScheme = signatureSchemeHandler;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSignature",
        policy => policy.Requirements.Add(new PrivateKeyRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, PrivateKeyRequirementHandler>();
builder.Services.AddScoped<IPrivateKeyValidator, PrivateKeyValidator>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/HealthCheck", () => "Healthy");

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "Emoney.ApiGateway" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

app.UseGatewayRateLimiter(configuration);

app.Run();