namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;

public class CreateDocumentToUserRequest
{
    public string UserId { get; set; }
    public Guid AgreementDocumentId { get; set; }
}
