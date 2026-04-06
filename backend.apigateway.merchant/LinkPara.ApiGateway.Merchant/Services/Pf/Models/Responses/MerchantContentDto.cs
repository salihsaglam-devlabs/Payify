using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantContentDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public List<MerchantContentVersionDto> Contents { get; set; }
}