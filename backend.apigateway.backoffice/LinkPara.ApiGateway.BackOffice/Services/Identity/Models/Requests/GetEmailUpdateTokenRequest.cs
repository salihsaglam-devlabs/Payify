using LinkPara.ApiGateway.BackOffice.Commons.Helpers;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class GetEmailUpdateTokenRequest
{
    public string NewEmail { get; set; }
}
public class GetEmailUpdateTokenServiceRequest : GetEmailUpdateTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
