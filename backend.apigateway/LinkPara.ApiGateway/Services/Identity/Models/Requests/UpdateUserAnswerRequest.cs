using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UpdateUserAnswerRequest
{
    public Guid SecurityQuestionId { get; set; }
    public string Answer { get; set; }
    public string CurrentAnswer { get; set; }
}
public class UpdateUserAnswerServiceRequest : UpdateUserAnswerRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
