using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;
public class GetPhoneNumberUpdateTokenRequest
{
    public string NewPhoneNumber { get; set; }
}
public class GetPhoneNumberUpdateTokenServiceRequest : GetPhoneNumberUpdateTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
