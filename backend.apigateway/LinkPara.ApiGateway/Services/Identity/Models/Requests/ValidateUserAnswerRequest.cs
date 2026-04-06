using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class ValidateUserAnswerRequest
{
    public string Answer { get; set; }
}
public class ValidateUserAnswerServiceRequest : ValidateUserAnswerRequest, IHasUserId
{
    public Guid UserId { get; set; }
}