using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UserAnswerRequest
{
    public Guid SecurityQuestionId { get; set; }
    public string Answer { get; set; }
}
public class UserAnswerServiceRequest : UserAnswerRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
