
using LinkPara.Documents.Application.Commons.Mappings;
using LinkPara.Documents.Domain.Entities;

using Microsoft.AspNetCore.Http;

namespace LinkPara.Documents.Application.Features.Documents
{
    public class DocumentDto : IMapFrom<Document>
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string OriginalFileName { get; set; }

        public Guid? UserId { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid DocumentTypeId { get; set; }
    }
}
