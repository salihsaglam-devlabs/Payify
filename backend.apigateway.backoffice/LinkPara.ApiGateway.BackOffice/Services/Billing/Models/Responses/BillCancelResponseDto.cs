namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class BillCancelResponseDto : BillingTransaction
{
    public BillCancelInvoice BillCancelInvoice { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}