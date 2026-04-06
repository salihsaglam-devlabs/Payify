namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;

public class CreateRuleResponse
{
    public Guid RuleId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}