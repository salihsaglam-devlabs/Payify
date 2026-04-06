namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

public class CreateDocumentToUserRequest
{
    public Guid UserId { get; set; }
    public Guid AgreementDocumentId { get; set; }
}