using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;


namespace LinkPara.Identity.Application.Features.AgreementDocuments
{
    public class AgreementDocumentResponse : IMapFrom<AgreementDocument>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastVersion { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public ProductType ProductType { get; set; }
        public List<DocumentVersionDto> Agreements { get; set; }
    }
}
