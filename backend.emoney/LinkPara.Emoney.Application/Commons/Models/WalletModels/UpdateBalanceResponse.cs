namespace LinkPara.Emoney.Application.Commons.Models.WalletModels;
public class UpdateBalanceResponse
{
    public string ResponseCode { get; set; }
    public string ResponseReasonCode { get; set; }
    public string Utid { get; set; }
    public string TransactionId { get; set; }
    public decimal CurrentBalance { get; set; }
}
