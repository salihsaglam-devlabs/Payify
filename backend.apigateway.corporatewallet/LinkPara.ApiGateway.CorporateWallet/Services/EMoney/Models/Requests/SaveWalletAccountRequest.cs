using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class SaveWalletAccountRequest
{
    public string WalletNumber { get; set; }
    public string Tag { get; set; }
}
public class SaveWalletAccountServiceRequest : SaveWalletAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}