namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
public class BulkTransferDetailRequest
{
    public string FullName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string Receiver { get; set; }
}