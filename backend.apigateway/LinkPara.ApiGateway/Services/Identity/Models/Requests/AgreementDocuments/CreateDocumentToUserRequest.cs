namespace LinkPara.ApiGateway.Services.Identity.Models.Requests.AgreementDocuments;

public class CreateDocumentToUserRequest
{
    public Guid UserId { get; set; }
    public Guid AgreementDocumentId { get; set; }
}
