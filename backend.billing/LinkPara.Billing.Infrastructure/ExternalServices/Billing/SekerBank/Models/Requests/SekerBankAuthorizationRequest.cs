namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankAuthorizationRequest
{
    public string username { get; set; }
    public string password { get; set; }
    public string grant_type { get; set; }
}
