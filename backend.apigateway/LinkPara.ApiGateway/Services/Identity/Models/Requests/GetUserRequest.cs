using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class GetUserRequest
{

}

public class GetUserServiceRequest : GetUserRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
