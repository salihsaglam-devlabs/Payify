using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments
{
    public class AgreementDocumentVersionResponse
    {
        public Guid Id { get; set; }
        public Guid AgreementDocumentId { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
        public string Content { get; set; }
        public bool IsLatest { get; set; }
        public bool IsOptional { get; set; }
        public bool IsForceUpdate { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
