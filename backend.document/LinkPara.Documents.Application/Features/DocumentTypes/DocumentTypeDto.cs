using LinkPara.Documents.Application.Commons.Mappings;
using LinkPara.Documents.Domain.Entities;

namespace LinkPara.Documents.Application.Features.DocumentTypes
{
    public class DocumentTypeDto : IMapFrom<DocumentType>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
