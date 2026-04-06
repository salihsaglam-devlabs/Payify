using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetUserWalletsFilterRequest { }

public class GetUserWalletsFilterServiceRequest : GetUserWalletsFilterRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
