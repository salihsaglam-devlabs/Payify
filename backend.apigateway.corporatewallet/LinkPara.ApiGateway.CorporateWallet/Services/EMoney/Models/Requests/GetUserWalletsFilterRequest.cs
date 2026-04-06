using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class GetUserWalletsFilterRequest { }

public class GetUserWalletsFilterServiceRequest : GetUserWalletsFilterRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
