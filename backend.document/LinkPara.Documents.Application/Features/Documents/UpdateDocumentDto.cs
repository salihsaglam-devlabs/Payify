using LinkPara.Documents.Application.Commons.Mappings;
using LinkPara.Documents.Domain.Entities;

namespace LinkPara.Documents.Application.Features.Documents;

public class UpdateDocumentDto : IMapFrom<Document>
{
    public Guid? Id { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string OriginalFileName { get; set; }

    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid DocumentTypeId { get; set; }
}
