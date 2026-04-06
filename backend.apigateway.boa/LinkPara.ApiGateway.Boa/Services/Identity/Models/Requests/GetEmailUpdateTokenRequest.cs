using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class GetEmailUpdateTokenRequest
{
    public string NewEmail { get; set; }
}
public class GetEmailUpdateTokenServiceRequest : GetEmailUpdateTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}