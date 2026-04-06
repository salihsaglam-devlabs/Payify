using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;

public class PhysicalPosEndOfDayDetailResponse
{
    public List<PhysicalPosUnacceptableTransactionDto> RelatedUnacceptableTransactions { get; set; }
    public PhysicalPosEndOfDayDto EndOfDay { get; set; }
    public List<MerchantTransactionDto> RelatedMerchantTransactions { get; set; }
    public List<ReconciliationTransactionDto> RelatedReconciliationTransactions { get; set; }
}