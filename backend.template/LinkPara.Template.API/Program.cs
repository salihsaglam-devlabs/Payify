using FluentValidation.AspNetCore;
using LinkPara.Template.Application;
using LinkPara.Template.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using LinkPara.SharedModels.Exceptions;
using System.Reflection;
using LinkPara.SharedModels.Localization;
using LinkPara.HttpProviders.Identity;
using LinkPara.SystemUser;
using Elastic.Apm.NetCoreAll;
using LinkPara.RateLimit;
using LinkPara.Template.Application.Commons.Models.ElasticApmConfiguration;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilterAttribute>())
    .AddFluentValidation(x => x.AutomaticValidationEnabled = false);

builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((options) =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(configuration["ServiceEndpoints:Identity"]);
});

var vaultClient = builder.Services.AddVault(configuration);

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
        .Enrich.WithProperty("ApplicationName","Template.API")
        .WriteTo.Debug()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions
            (new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "SerilogSettings", "NodeUri")))
            {
                IndexFormat = "exception-logs-{0:yyyy.MM}",
                AutoRegisterTemplate = true
            })
        .CreateLogger());
});

builder.Services.ConfigureLocalization(); // Required for localization

var app = builder.Build();

app.ConfigureLocalization(); // Required for localization

app.ConfigureApplicationUser("Template");

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
        { "ElasticApm:ServiceName", "Template.API" }
    });

var isApmEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ApmEnabled");
if (isApmEnabled)
{
    app.UseAllElasticApm(configuration);
}

app.UseMicroServiceRateLimiter(configuration);

app.Run();