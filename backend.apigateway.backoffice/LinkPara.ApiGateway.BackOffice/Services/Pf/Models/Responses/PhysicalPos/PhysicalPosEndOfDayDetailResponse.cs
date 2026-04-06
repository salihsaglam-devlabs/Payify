namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class PhysicalPosEndOfDayDetailResponse
{
    public List<PhysicalPosUnacceptableTransactionDto> RelatedUnacceptableTransactions { get; set; }
    public PhysicalPosEndOfDayDto EndOfDay { get; set; }
    public List<MerchantTransactionDto> RelatedMerchantTransactions { get; set; }
    public List<ReconciliationTransactionDto> RelatedReconciliationTransactions { get; set; }
}