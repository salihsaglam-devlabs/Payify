namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class EmoneyReportDto
{
    public int TotalWallet { get; set; }
    public int ActiveWallet { get; set; }
    public int PassiveWallet { get; set; }
    public int SucceededTransaction { get; set; }
    public decimal SucceededTransactionAmount { get; set; }
    public int FailedTransaction { get; set; }
    public decimal FailedTransactionAmount { get; set; }
    public int CancelledTransaction { get; set; }
    public decimal CancelledTransactionAmount { get; set; }
    public int PendingApprovalTransaction { get; set; }
    public int FailedFutureDateTransferOrder { get; set; }
    public decimal FailedFutureDateTransferAmount { get; set; }
}
