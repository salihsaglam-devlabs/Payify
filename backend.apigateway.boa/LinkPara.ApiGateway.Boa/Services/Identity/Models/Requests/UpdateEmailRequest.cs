using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class UpdateEmailRequest
{
    public string Token { get; set; }
    public string NewEmail { get; set; }
}

public class UpdateEmailServiceRequest : UpdateEmailRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
