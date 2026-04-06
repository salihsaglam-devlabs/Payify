using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class GetEmailUpdateTokenRequest
{
    public string NewEmail { get; set; }
}
public class GetEmailUpdateTokenServiceRequest : GetEmailUpdateTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}