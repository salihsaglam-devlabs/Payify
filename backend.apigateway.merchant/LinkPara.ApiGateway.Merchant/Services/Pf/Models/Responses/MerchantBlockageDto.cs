using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantBlockageDto
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public MerchantBlockageStatus MerchantBlockageStatus { get; set; }

    public Guid MerchantId { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public List<MerchantBlockageDetailDto> MerchantBlockageDetails { get; set; }
    public List<PostingBalanceDto> PostingBalances { get; set; }
}
