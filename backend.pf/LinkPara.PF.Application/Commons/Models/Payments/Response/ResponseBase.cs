namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class ResponseBase
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ConversationId { get; set; }
}
