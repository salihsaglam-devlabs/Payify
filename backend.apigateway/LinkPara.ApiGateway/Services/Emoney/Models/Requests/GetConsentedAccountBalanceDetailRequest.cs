using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetConsentedAccountBalanceDetailRequest
{
    public string AccountReference { get; set; } 
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
}

