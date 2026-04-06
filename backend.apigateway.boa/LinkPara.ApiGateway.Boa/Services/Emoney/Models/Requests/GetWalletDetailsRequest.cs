using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetWalletDetailsRequest
{
    public Guid WalletId { get; set; }
}

public class GetWalletDetailsServiceRequest : GetWalletDetailsRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
