namespace LinkPara.PF.Pos.ApiGateway.Models.Responses;

public class SignatureTestDataResponse
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
    public string SerialNumber { get; set; }
    public Guid MerchantId { get; set; }
}