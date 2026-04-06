namespace LinkPara.HttpProviders.Emoney.Models;

public class ProvisionPreviewRequest
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
}
