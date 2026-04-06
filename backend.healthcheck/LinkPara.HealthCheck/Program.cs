using LinkPara.HealthCheck.HttpCheck;
using LinkPara.HealthCheck.Models;
using LinkPara.HealthCheck.Services;
using LinkPara.RateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using LinkPara.HealthCheck.Utility;
using MassTransit;
using System.Reflection;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var configuration = builder.Configuration;

var vaultClient = builder.Services.AddVault(configuration);
var serviceUrls = new HealthCheckConfig();
serviceUrls = vaultClient.GetSecretValue<HealthCheckConfig>("SharedSecrets", "ServiceUrls", null);

//get eventbus from vault
var eventBusSettings = new EventBusSettings();
eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);
serviceUrls.RabbitMQConnectionString = $"amqp://{eventBusSettings.Username}:{eventBusSettings.Password}@{eventBusSettings.Host}/";

//get elasticsearch url from vault
serviceUrls.ElasticSearchUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "SerilogSettings", "NodeUri");

builder.Services.AddLogging();

builder.Services.AddHealthChecks()

                .AddRabbitMQ(
                    (Func<IServiceProvider, IConnection>)(sp =>
                    {
                        var factory = new ConnectionFactory
                        {
                            Uri = new Uri(serviceUrls.RabbitMQConnectionString)
                        };
                        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                    }),
                    name: "RabbitMQ",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { nameof(Tags.RabbitMQ), nameof(Tags.Queue), nameof(Tags.Tools) }
                )

                .AddElasticsearch(
                    serviceUrls.ElasticSearchUrl,
                    name: "Elastic Search",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { nameof(Tags.ElasticSearch), nameof(Tags.Log), nameof(Tags.Tools) })

                .AddTypeActivatedCheck<AccountingApiCheck>(
                    name: "Accounting.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Accounting },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<GatewayApiCheck>(
                    "ApiGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.ApiGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<ApprovalApiCheck>(
                    "Approval.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Approval },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<BackOfficeGatewayApiCheck>(
                    "BackOfficeGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.BackOfficeGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<BTransApiCheck>(
                    "BTrans.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.BTrans },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<BillingApiCheck>(
                    "Billing.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Billing },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<BusinessParameterApiCheck>(
                    "BusinessParameter.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.BusinessParameter },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<CalendarApiCheck>(
                    "Calendar.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Calendar },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<CampaignManagementApiCheck>(
                    "CampaignManagement.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.CampaignManagement },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<ContentApiCheck>(
                    "Content.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Content },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<CustomerManagementApiCheck>(
                    "CustomerManagement.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.CustomerManagement },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<DigitalKycApiCheck>(
                    "DigitalKyc.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.DigitalKyc },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<DocumentApiCheck>(
                    "Documents.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Document },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<EmoneyApiCheck>(
                    "Emoney.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Emoney },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<EmoneyApiGatewayApiCheck>(
                    "EmoneyApiGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.EmoneyApiGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<EpinApiCheck>(
                    "Epin.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Epin },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<FraudApiCheck>(
                    "Fraud.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Fraud },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<IKSApiCheck>(
                    "IKS.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.IKS },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<IWalletApiGatewayCheck>(
                    "IWalletApiGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.IWalletGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<IdentityApiCheck>(
                    "Identity.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Identity },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<KKBApiCheck>(
                    "KKB.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.KKB },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<KPSApiCheck>(
                    "KPS.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.KPS },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<LocationApiCheck>(
                    "Location.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Location },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<LogConsumersCheck>(
                    "LogConsumers",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.LogConsumers },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<MerchantGatewayApiCheck>(
                    "MerchantApiGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.MerchantGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<MoneyTransferApiCheck>(
                    "MoneyTransfer.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.MoneyTransfer },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<NotificationApiCheck>(
                    "Notification",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Notification },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<PFApiCheck>(
                    "PF.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Pf },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<PFGatewayApiCheck>(
                    "PFGateway.API",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.PFApiGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<SchedulerCheck>(
                    "Scheduler",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.Scheduler },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API), nameof(Tags.Scheduler) })

                .AddTypeActivatedCheck<AlertingSystemApiCheck>(
                    "AlertingSystem",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.AlertingSystem },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<SoftOtpApiCheck>(
                    "SoftOtp",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.SoftOtp },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) })

                .AddTypeActivatedCheck<PFPageGatewayApiCheck>(
                    "PfPageGateway",
                    failureStatus: HealthStatus.Unhealthy,
                    args: new[] { serviceUrls.PfPageGateway },
                    tags: new[] { nameof(Tags.Service), nameof(Tags.API) });



builder.Services.AddHealthChecksUI((opt) =>
{
    opt.AddWebhookNotification(
        "Email",
        "http://localhost:5025/Webhooks/",
        JsonSerializer.Serialize(
            new Dictionary<string, object> {
                { "Description", "[[DESCRIPTIONS]]" },
                { "Message", "[[FAILURE]]" },
                { "Service", "[[LIVENESS]]" }
            }
        ),
        "{ \"message\": \"[[LIVENESS]] is back to life\"}");

}).AddInMemoryStorage();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.AddBus(context => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
                           h =>
                           {
                               h.Username(eventBusSettings.Username);
                               h.Password(eventBusSettings.Password);
                           });
    }));
});
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ISecurityService, SecurityService>();

builder.WebHost.UseKestrel(opts =>
{
    opts.ListenAnyIP(5025);
});

var app = builder.Build();

app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapControllers();
app.MapHealthChecks("/tools-health", MapOptions.GetOptions(Tags.Tools));
app.MapHealthChecks("/services-health", MapOptions.GetOptions(Tags.Service));
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health";
    options.AddCustomStylesheet("./Assets/health-check-blue.css");
});

app.UseMicroServiceRateLimiter(configuration);

app.Run();

