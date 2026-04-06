using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.Posting;

public class PostingPfProfitDto : IMapFrom<PostingPfProfit>
{
    public Guid Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfNetCommissionAmount { get; set; }
    public decimal ProtectionTransferAmount { get; set; }
    public int Currency { get; set; }
    public List<PostingPfProfitDetailDto> PostingPfProfitDetails { get; set; }
}