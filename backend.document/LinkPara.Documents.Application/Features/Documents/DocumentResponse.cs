using LinkPara.Documents.Application.Commons.Mappings;
using LinkPara.Documents.Application.Features.DocumentTypes;
using LinkPara.Documents.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Documents.Application.Features.Documents
{
    public class DocumentResponse : IMapFrom<Document>
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid? UserId { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }

        public Guid DocumentTypeId { get; set; }
        public virtual DocumentTypeDto DocumentType { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
