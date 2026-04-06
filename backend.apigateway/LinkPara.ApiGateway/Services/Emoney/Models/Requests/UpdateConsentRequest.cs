using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class UpdateConsentRequest
{
    public string ConsentId { get; set; }
    public string CustomerId { get; set; }
    public ConsentType ConsentTypeValue { get; set; }
    public List<UpdateConsentAccount> Accounts { get; set; }
    public string CustomerName { get; set; }
    public string IdentityType { get; set; }
    public string IdentityValue { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }

}

