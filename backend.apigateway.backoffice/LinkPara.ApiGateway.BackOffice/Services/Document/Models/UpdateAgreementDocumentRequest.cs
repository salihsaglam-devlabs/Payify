using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Document.Models
{
    public class UpdateAgreementDocumentRequest
    {
        public Guid AgreementDocumentId { get; set; }
        public string Version { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public List<DocumentVersionDto> Agreements { get; set; }
    }
}
