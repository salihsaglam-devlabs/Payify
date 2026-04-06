namespace LinkPara.Billing.Application.Commons.Models.ExternalServiceConfiguration;

public class SekerBankServiceSettings
{
    public string ServiceUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string GrantType { get; set; }
    public string ContentType { get; set; }
    public string FormContentType { get; set; }
    public int InquiryTimeout { get; set; }
    public int PaymentTimeout { get; set; }
    public int CancellationTimeout { get; set; }
    public int ReconciliationTimeout { get; set; }
    public int TechnicalTimeout { get; set; }
    public int AuthTokenInterval { get; set; }
}