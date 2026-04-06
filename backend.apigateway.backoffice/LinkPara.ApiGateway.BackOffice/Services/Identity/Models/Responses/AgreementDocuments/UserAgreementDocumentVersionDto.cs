namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;

public class UserAgreementDocumentVersionDto
{
    public string UserId { get; set; }
    public string AgreementDocumentName { get; set; }
    public string Version { get; set; }
    public string Content { get; set; }
    public bool IsForceUpdate { get; set; }
}
