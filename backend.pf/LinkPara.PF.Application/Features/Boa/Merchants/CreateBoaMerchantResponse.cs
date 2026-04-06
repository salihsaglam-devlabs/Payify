using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Boa.Merchants;

public class CreateBoaMerchantResponse
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string MerchantNumber { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public MerchantApiKeyDto MerchantApiKey { get; set; }
}