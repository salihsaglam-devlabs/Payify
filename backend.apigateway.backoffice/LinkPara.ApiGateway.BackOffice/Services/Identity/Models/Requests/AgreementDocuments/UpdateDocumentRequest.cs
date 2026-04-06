namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;

public class UpdateDocumentRequest
{
    public Guid AgreementDocumentId { get; set; }
    public string Name { get; set; }
    public string LanguageCode { get; set; }
    public bool RecordStatus { get; set; }
}
