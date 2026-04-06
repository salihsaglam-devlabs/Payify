using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class UpdatePhoneNumberRequest
{
    public string Token { get; set; }
    public string NewPhoneNumber { get; set; }
    public string NewPhoneCode { get; set; }
}

public class UpdatePhoneNumberServiceRequest : UpdatePhoneNumberRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
