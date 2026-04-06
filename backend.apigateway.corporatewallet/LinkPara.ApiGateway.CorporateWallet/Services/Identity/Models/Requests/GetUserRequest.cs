using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

public class GetUserRequest
{

}

public class GetUserServiceRequest : GetUserRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
