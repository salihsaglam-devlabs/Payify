namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class ResponseModel
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ConversationId { get; set; }
}