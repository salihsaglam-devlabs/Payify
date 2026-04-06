namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconciliationInstitutionPaymentDetailsSummaryResponse : Reconciliation
{
    public Guid InstitutionId { get; set; }
    public List<PaymentTransaction> PaymentTransactions { get; set; }
}