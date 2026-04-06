using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class AccountUnlinkRequest
{
    public string AccountKey { get; set; }
    public string OrderNo { get; set; }
}

public class AccountUnlinkServiceRequest : AccountUnlinkRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
