using System.Reflection;
using System.Text.Json.Serialization;
using Elastic.Apm.Api;
using Elastic.Apm.NetCoreAll;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Pos.ApiGateway;
using LinkPara.PF.Pos.ApiGateway.Authentication;
using LinkPara.PF.Pos.ApiGateway.Authentication.SignaturePolicy;
using LinkPara.PF.Pos.ApiGateway.Commons;
using LinkPara.PF.Pos.ApiGateway.Commons.Filters;
using LinkPara.PF.Pos.ApiGateway.Filters;
using LinkPara.PF.Pos.ApiGateway.Models;
using LinkPara.PF.Pos.ApiGateway.Services;
using LinkPara.RateLimit;
using LinkPara.Security;
using LinkPara.SharedModels.Localization;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using Serilog.Sinks.RabbitMQ;
using LinkPara.SharedModels.Persistence;
using LinkPara.PF.Pos.ApiGateway.Filters.RequestResponseLogging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<PfExceptionFilterAttribute>();
        options.Filters.Add<ResponseModelStatusCodeFilter>();
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "PF POS Gateway API", Version = "v1", });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var vaultClient = builder.Services.AddVault(configuration);

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
        .Enrich.WithProperty("ApplicationName","LinkPara.PF.Pos.ApiGateway")
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

await builder.Services.AddApplicationUser(vaultClient);

builder.Services.RegisterHttpClients(vaultClient);
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddCors();

const string signatureSchemeHandler = "DefaultHandler";

builder.Services.AddAuthentication(options =>
{
    options.AddScheme<SignatureSchemeHandler>(signatureSchemeHandler, signatureSchemeHandler);
    options.DefaultChallengeScheme = signatureSchemeHandler;
    options.DefaultForbidScheme = signatureSchemeHandler;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSignature",
        policy => policy.Requirements.Add(new SignatureRequirement()));
});

builder.Services.AddInfrastructure(configuration,vaultClient);
builder.Services.ConfigureLocalization(); // Required for localization

var app = builder.Build();

app.ConfigureLocalization(); // Required for localization

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
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

if (app.Environment.IsProduction())
{
    app.UseCors(x =>
    {
        x.AllowAnyOrigin();
        x.AllowAnyMethod();
        x.AllowAnyHeader();
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseRequestResponseLoggingMiddleware();

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "PF.Pos.ApiGateway" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}
app.UseGatewayRateLimiter(vaultClient.GetSecretValue<RateLimiterSettings>("SharedSecrets", "RateLimitingSettings"),
                          Gateway.PFPageGateway.ToString(),
                          app.Services.GetRequiredService<ILoggerFactory>());

app.MapGet("/HealthCheck", () => "Healthy");

app.Run();