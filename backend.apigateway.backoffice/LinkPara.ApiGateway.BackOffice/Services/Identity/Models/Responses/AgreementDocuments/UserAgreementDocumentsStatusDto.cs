namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;

public class UserAgreementDocumentsStatusDto
{
    public Guid AgreementDocumentId { get; set; }
    public string AgreementDocumentName { get; set; }
    public string Version { get; set; }
    public bool IsLatest { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool IsSigned { get; set; }
    public string Content { get; set; }
}
