using LinkPara.HttpProviders.PF.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;

public class CreateBoaMerchantResponse
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string MerchantNumber { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public MerchantApiKeyDto MerchantApiKey { get; set; }
}