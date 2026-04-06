using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.AgreementDocuments
{
    public class AgreementDocumentDto : IMapFrom<AgreementDocument>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public List<DocumentVersionDto> Agreements { get; set; }
    }
}
