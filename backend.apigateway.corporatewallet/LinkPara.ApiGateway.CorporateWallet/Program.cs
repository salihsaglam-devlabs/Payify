using Elastic.Apm.NetCoreAll;
using LinkPara.ApiGateway.CorporateWallet.Authorization;
using LinkPara.ApiGateway.CorporateWallet.Authorization.CustomAuthorizations;
using LinkPara.ApiGateway.CorporateWallet.Authorization.CustomPolicies.OtpPolicy;
using LinkPara.ApiGateway.CorporateWallet.Authorization.Scopes;
using LinkPara.ApiGateway.CorporateWallet.Commons;
using LinkPara.ApiGateway.CorporateWallet.Commons.ElasticApmConfiguration;
using LinkPara.ApiGateway.CorporateWallet.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Filters.RequestResponseLogging;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using Serilog.Sinks.RabbitMQ;
using LinkPara.RateLimit;
using LinkPara.SharedModels.Persistence;
using System.Text.Json.Serialization;
using LinkPara.HttpProviders.Identity;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddControllers(options =>
        options.Filters.Add<ApiGatewayExceptionFilterAttribute>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Corporate Gateway API", Version = "v1" });
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
    option.OperationFilter<OtpRequirementOperationFilter>();

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
        .Enrich.WithProperty("ApplicationName", "LinkPara.ApiGateway.CorporateWallet")
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

builder.Services.AddInfrastructure(configuration, vaultClient);
builder.Services.AddScoped<IServiceRequestConverter, ServiceRequestConverter>();
builder.Services.AddScoped<IUserNameGenerator, UserNameGenerator>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddCors();
builder.Services.AddJwtAuthorization(configuration, vaultClient);
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();

const string otpSchemeHandler = "DefaultHandler";

builder.Services.AddAuthentication(options =>
{
    options.AddScheme<OtpSchemeHandler>(otpSchemeHandler, otpSchemeHandler);
    options.DefaultChallengeScheme = otpSchemeHandler;
    options.DefaultForbidScheme = otpSchemeHandler;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireOtp",
        policy => policy.Requirements.Add(new OtpRequirement()));
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, OtpRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();

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

app.MapGet("/", () => "Healthy");
app.MapGet("/healthcheck", () => "Healthy");

app.UseRequestResponseLoggingMiddleware();

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "ApiGateway.CorporateWallet" }
    });



var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

app.UseGatewayRateLimiter(vaultClient.GetSecretValue<RateLimiterSettings>("SharedSecrets", "RateLimitingSettings"),
                          Gateway.APIGateway.ToString(),
                          app.Services.GetRequiredService<ILoggerFactory>());

app.Run();