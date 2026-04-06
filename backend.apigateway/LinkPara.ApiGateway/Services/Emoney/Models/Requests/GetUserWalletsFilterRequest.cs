using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetUserWalletsFilterRequest { }

public class GetUserWalletsFilterServiceRequest : GetUserWalletsFilterRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
