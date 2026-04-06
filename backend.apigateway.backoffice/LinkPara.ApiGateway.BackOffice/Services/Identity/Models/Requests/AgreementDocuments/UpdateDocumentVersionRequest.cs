namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;

public class UpdateDocumentVersionRequest
{
    public Guid AgreementDocumentVersionId { get; set; }
    public string Version { get; set; }
    public string Content { get; set; }
    public bool IsLatest { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool RecordStatus { get; set; }

}