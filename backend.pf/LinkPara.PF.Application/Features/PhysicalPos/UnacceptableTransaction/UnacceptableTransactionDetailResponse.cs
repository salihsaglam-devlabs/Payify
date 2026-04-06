using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;

public class UnacceptableTransactionDetailResponse
{
    public PhysicalPosUnacceptableTransactionDto UnacceptableTransaction { get; set; }
    public PhysicalPosEndOfDayDto RelatedEndOfDay { get; set; }
    public MerchantTransactionDto RelatedMerchantTransaction { get; set; }
    public ReconciliationTransactionDto RelatedReconciliationTransaction { get; set; }
}