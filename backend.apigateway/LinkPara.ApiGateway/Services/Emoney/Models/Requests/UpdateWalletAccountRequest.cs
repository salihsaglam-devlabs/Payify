using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class UpdateWalletAccountRequest
{
    public string Tag { get; set; }
    public string WalletNumber { get; set; }
}

public class UpdateWalletAccountServiceRequest : UpdateWalletAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
