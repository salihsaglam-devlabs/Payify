using System.ComponentModel.DataAnnotations.Schema;

using LinkPara.SharedModels.Persistence;

namespace LinkPara.Documents.Domain.Entities
{
    public class Document : AuditEntity, ITrackChange
    {
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }

        public Guid? UserId { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }
        public Guid? AccountId { get; set; }

        public Guid DocumentTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }
    }
}
