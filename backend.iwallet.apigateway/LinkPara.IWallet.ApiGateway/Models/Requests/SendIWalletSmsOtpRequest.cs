namespace LinkPara.IWallet.ApiGateway.Models.Requests;

public class SendIWalletSmsOtpRequest
{
  public string WalletNumber { get; set; }
  public string Amount { get; set; }
  public string MerchantName { get; set; }
  public string OtpPassword { get; set; }
  public int Type { get; set; }
}
