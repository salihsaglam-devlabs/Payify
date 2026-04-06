namespace LinkPara.HttpProviders.Emoney.Models;

public class ProvisionCashbackRequest
{
    public string ProvisionReference { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }
}
