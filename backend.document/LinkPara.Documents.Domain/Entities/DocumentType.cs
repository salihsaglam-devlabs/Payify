using System.ComponentModel.DataAnnotations.Schema;

using LinkPara.SharedModels.Persistence;

namespace LinkPara.Documents.Domain.Entities
{
    public class DocumentType : AuditEntity, ITrackChange
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
