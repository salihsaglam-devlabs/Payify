using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantResponse
{
    public Guid MerchantId { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
}
