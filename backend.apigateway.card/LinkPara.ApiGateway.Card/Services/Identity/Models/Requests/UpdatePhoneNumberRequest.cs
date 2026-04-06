using LinkPara.ApiGateway.Card.Commons.Helpers;

namespace LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;

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
