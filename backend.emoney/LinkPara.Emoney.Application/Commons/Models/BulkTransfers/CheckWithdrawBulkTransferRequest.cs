namespace LinkPara.Emoney.Application.Commons.Models.BulkTransfers;

public class CheckWithdrawBulkTransferRequest
{
    public Guid TransactionId { get; set; }
    public TransactionResult TransactionResult { get; set; }
}

public enum TransactionResult
{
    Success,
    Returned,
    Failed
}
