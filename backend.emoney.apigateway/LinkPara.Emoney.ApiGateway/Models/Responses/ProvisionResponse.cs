namespace LinkPara.Emoney.ApiGateway.Models.Responses;

public class ProvisionResponse
{
    public string ConversationId { get; set; }
    public string ReferenceNumber { get; set; }
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ProvisionReference { get; set; }
}
