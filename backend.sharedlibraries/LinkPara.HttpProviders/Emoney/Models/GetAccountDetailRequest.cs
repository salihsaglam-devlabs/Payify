namespace LinkPara.HttpProviders.Emoney.Models;

public class GetAccountDetailRequest
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}
