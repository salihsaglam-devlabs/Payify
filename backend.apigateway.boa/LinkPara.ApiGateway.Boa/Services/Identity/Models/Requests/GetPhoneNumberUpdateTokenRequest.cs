using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class GetPhoneNumberUpdateTokenRequest
{
    public string NewPhoneNumber { get; set; }
}
public class GetPhoneNumberUpdateTokenServiceRequest : GetPhoneNumberUpdateTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
