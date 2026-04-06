namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankAuthorizationResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public DateTime register_date { get; set; }
    public string username { get; set; }
    public string jti { get; set; }
}