namespace LinkPara.HealthCheck.Models;

public class HealthCheckConfig
{
    public string RabbitMQConnectionString { get; set; }
    public string ElasticSearchUrl { get; set; }
    public string Accounting { get; set; } = "http://unusedservice.com";
    public string ApiGateway { get; set; } = "http://unusedservice.com";
    public string Approval { get; set; } = "http://unusedservice.com";
    public string BackOfficeGateway { get; set; } = "http://unusedservice.com";
    public string BTrans { get; set; } = "http://unusedservice.com";
    public string Billing { get; set; } = "http://unusedservice.com";
    public string BusinessParameter { get; set; } = "http://unusedservice.com";
    public string Calendar { get; set; } = "http://unusedservice.com";
    public string CampaignManagement { get; set; } = "http://unusedservice.com";
    public string Content { get; set; } = "http://unusedservice.com";
    public string CustomerManagement { get; set; } = "http://unusedservice.com";
    public string DigitalKyc { get; set; } = "http://unusedservice.com";
    public string Document { get; set; } = "http://unusedservice.com";
    public string Emoney { get; set; } = "http://unusedservice.com";
    public string EmoneyApiGateway { get; set; } = "http://unusedservice.com";
    public string Epin { get; set; } = "http://unusedservice.com";
    public string Fraud { get; set; } = "http://unusedservice.com";
    public string IKS { get; set; } = "http://unusedservice.com";
    public string IWalletGateway { get; set; } = "http://unusedservice.com";
    public string Identity { get; set; } = "http://unusedservice.com";
    public string KKB { get; set; } = "http://unusedservice.com";
    public string KPS { get; set; } = "http://unusedservice.com";
    public string Location { get; set; } = "http://unusedservice.com";
    public string LogConsumers { get; set; } = "http://unusedservice.com";
    public string MerchantGateway { get; set; } = "http://unusedservice.com";
    public string MoneyTransfer { get; set; } = "http://unusedservice.com";
    public string Notification { get; set; } = "http://unusedservice.com";
    public string PFApiGateway { get; set; } = "http://unusedservice.com";
    public string Pf { get; set; } = "http://unusedservice.com";
    public string Scheduler { get; set; } = "http://unusedservice.com";
    public string AlertingSystem { get; set; } = "http://unusedservice.com";
    public string SoftOtp { get; set; } = "http://unusedservice.com";
    public string PfPageGateway { get; set; } = "http://unusedservice.com";
}
