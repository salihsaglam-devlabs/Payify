namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class MoneyTransferReportDto
{
    public int SucceededIncomingTransaction { get; set; }
    public decimal SucceededIncomingTransactionAmount { get; set; }
    public int FailedIncomingTransaction { get; set; }
    public decimal FailedIncomingTransactionAmount { get; set; }
    public int SucceededOutgoingTransaction { get; set; }
    public decimal SucceededOutgoingTransactionAmount { get; set; }
    public int FailedOutgoingTransaction { get; set; }
    public decimal FailedOutgoingTransactionAmount { get; set; }
    public int SucceededReturnedTransaction { get; set; }
    public decimal SucceededReturnedTransactionAmount { get; set; }
    public int FailedReturnedTransaction { get; set; }
    public decimal FailedReturnedTransactionAmount { get; set; }
    public int ActionRequiredIncomingTransaction { get; set; }
    public int ActionRequiredOutgoingTransaction { get; set; }
    public int ActionRequiredReturnedTransaction { get; set; }
    public int PendingReconciliation { get; set; }
}
