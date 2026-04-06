using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.Pos.ApiGateway.Models.Requests;

public class RequestHeader
{
    [FromHeader]
    public string PublicKey { get; set; }
    [FromHeader]
    public string Nonce { get; set; }
    [FromHeader]
    public string Signature { get; set; }
    [FromHeader]
    public string ConversationId { get; set; }
    [FromHeader]
    public string[] ClientIpAddress { get; set; }
    [FromHeader]
    public string SerialNumber { get; set; }
}