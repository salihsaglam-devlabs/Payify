namespace LinkPara.PF.Pos.ApiGateway.Models.Requests;

public class SignatureTestDataRequest
{
    public string PublicKey { get; set; }
    public string SerialNumber { get; set; }
    public string ConversationId { get; set; }
    public string ProductionAccessKey { get; set; }
}