namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class Verify3dsResponse: ResponseBase
{
    public string CallbackUrl { get; set; }
    public string MdStatus { get; set; }
    public string ThreeDSessionId { get; set; }
    public bool HalfSecure { get; set; }
    public string ConversationId { get; set; }
}