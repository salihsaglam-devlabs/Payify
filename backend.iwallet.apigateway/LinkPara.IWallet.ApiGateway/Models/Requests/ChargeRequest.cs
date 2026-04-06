namespace LinkPara.IWallet.ApiGateway.Models.Requests;

public class ChargeRequest
{
    public string WalletId { get; set; }
    public int TerminalId { get; set; }
    public string TerminalName { get; set; }
    public int CurrencyCode { get; set; }
    public int QrCode { get; set; }
    public decimal Amount { get; set; }
}
