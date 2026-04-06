using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class SaveWalletAccountRequest
{
    public string WalletNumber { get; set; }
    public string Tag { get; set; }
    public string ReceiverName { get; set; }
}
public class SaveWalletAccountServiceRequest : SaveWalletAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}