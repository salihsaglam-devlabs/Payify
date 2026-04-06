using LinkPara.ApiGateway.BackOffice.Authorization;
using LinkPara.ApiGateway.BackOffice.Authorization.Scopes;
using LinkPara.ApiGateway.BackOffice.Authorization.CustomAuthorizations;
using LinkPara.ApiGateway.BackOffice.Authorization.CustomPolicies.OtpPolicy;
using LinkPara.ApiGateway.BackOffice.Filters.RequestResponseLogging;
using LinkPara.ApiGateway.BackOffice.Filters.MakerChecker;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using LinkPara.SharedModels.Localization;
using LinkPara.SharedModels.Exceptions;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;
using Newtonsoft.Json.Converters;
using LinkPara.Cache;
using LinkPara.Security;
using Elastic.Apm.NetCoreAll;
using LinkPara.ApiGateway.BackOffice.Commons;
using LinkPara.ApiGateway.BackOffice.Commons.ElasticApmConfiguration;
using LinkPara.ApiGateway.BackOffice.Services.BackOffice.HttpClients;
using LinkPara.RateLimit;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ContextProvider;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using EmitEventFailureHandling = Serilog.Sinks.RabbitMQ.EmitEventFailureHandling;
using LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;
using Serilog.Sinks.RabbitMQ;
using LinkPara.ApiGateway.BackOffice.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.BackOffice.Filters.LoginActionFilter;
using LinkPara.SharedModels.Persistence;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<LinkPara.ApiGateway.BackOffice.Authorization.CustomAuthorizations.AuthorizeAttribute>();
        options.Filters.Add<ApiGatewayExceptionFilterAttribute>();
    }).AddNewtonsoftJson(options =>
        options.SerializerSettings.Converters.Add(new StringEnumConverter()));

builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "BackOfficeGateway API", Version = "v1" });
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
        .Enrich.WithProperty("ApplicationName", "LinkPara.ApiGateway.BackOffice")
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
builder.Services.AddScoped<ISignatureGenerator, SignatureGenerator>();
builder.Services.AddScoped<IContextProvider, CurrentContextProvider>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddCors();
builder.Services.AddJwtAuthorization(configuration, vaultClient);

builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICaseContainer, CaseContainer>();
builder.Services.AddSingleton<IHashGenerator, HashGenerator>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ISecureKeyGenerator, SecureKeyGenerator>();
builder.Services.AddScoped<IUserNameGenerator, UserNameGenerator>();
builder.Services.AddScoped<ISecureRandomGenerator, SecureRandomGenerator>();
builder.Services.AddScoped<LoginActionFilter>();
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

//if (app.Environment.IsProduction())
//{
//    app.UseCors(x =>
//    {
//        x.AllowAnyOrigin();
//        x.AllowAnyMethod();
//        x.AllowAnyHeader();
//    });
//}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/healthcheck", () => "Healthy");

app.UseRequestResponseLoggingMiddleware();
app.UseMakerCheckerMiddleware();

var elasticApmSettings = vaultClient.GetSecretValue<ElasticApmSettings>("SharedSecrets", "ElasticApmSettings", null);
configuration.AddInMemoryCollection(
    new Dictionary<string, string>
    {
        { "ElasticApm:Environment", elasticApmSettings.Environment },
        { "ElasticApm:SecretToken", elasticApmSettings.SecretToken },
        { "ElasticApm:ServerUrl", elasticApmSettings.ServerUrl },
        { "ElasticApm:ServiceVersion", elasticApmSettings.ServiceVersion },
        { "ElasticApm:ServiceName", "ApiGateway_BackOffice" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

app.UseGatewayRateLimiter(vaultClient.GetSecretValue<RateLimiterSettings>("SharedSecrets", "RateLimitingSettings"),
                          Gateway.BackOffice.ToString(),
                          app.Services.GetRequiredService<ILoggerFactory>());

app.Run();