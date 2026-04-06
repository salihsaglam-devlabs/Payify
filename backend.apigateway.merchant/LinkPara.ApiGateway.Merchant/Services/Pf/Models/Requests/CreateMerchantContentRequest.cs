using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class CreateMerchantContentRequest
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public List<MerchantContentVersionDto> Contents { get; set; }
}