using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.AgreementDocuments
{
    public class DocumentVersionDto : IMapFrom<AgreementDocumentVersion>
    {
        public Guid Id { get; set; }
        public Guid AgreementDocumentId { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
        public string Content { get; set; }
        public bool IsLatest { get; set; }
        public bool IsOptional { get; set; }
        public bool IsForceUpdate { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
