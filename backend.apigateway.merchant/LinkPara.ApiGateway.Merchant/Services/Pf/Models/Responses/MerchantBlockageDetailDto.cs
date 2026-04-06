using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantBlockageDetailDto
{
    public Guid Id { get; set; }
    public DateTime PostingDate { get; set; }
    public Guid MerchantId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
}
