using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UpdateEmailRequest
{
    public string NewEmail { get; set; }
    public string Token { get; set; }

}
public class UpdateEmailServiceRequest : UpdateEmailRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
