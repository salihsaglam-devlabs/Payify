namespace LinkPara.HttpProviders.Identity.Models;

public class CreateUserAnswerRequest
{
    public Guid UserId { get; set; }
    public Guid SecurityQuestionId { get; set; }
    public string Answer { get; set; }
}
