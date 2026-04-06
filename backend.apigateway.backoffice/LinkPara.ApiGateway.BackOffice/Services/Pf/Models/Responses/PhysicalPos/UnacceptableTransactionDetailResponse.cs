namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class UnacceptableTransactionDetailResponse
{
    public PhysicalPosUnacceptableTransactionDto UnacceptableTransaction { get; set; }
    public PhysicalPosEndOfDayDto RelatedEndOfDay { get; set; }
    public MerchantTransactionDto RelatedMerchantTransaction { get; set; }
    public ReconciliationTransactionDto RelatedReconciliationTransaction { get; set; }
}