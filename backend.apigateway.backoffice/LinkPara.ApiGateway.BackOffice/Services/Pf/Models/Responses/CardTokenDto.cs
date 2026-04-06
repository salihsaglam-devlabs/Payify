namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CardTokenDto : ResponseModel
{
    public string CardToken { get; set; }
    public string Signature { get; set; }
}

public class ResponseModel
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ConversationId { get; set; }
}
