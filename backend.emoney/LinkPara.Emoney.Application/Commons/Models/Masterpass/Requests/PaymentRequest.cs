namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class PaymentRequest
{
    public string Amount { get; set; }
    public int MerchantId { get; set; }
    public string OrderId { get; set; }
    public string AccountKey { get; set; }
    public int InstallmentCount { get; set; }
    public string RequestReferenceNo { get; set; }
    public string TerminalGroupId { get; set; }
    public string AcquirerIcaNumber { get; set; }
    public string Token { get; set; }
}
