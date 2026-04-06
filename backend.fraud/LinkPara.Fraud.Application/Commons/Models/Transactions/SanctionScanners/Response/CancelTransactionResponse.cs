namespace LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;

public class CancelTransactionResponse : BaseResponse
{
    public object ExtraInfo { get; set; }
    public string Result { get; set; }
}
