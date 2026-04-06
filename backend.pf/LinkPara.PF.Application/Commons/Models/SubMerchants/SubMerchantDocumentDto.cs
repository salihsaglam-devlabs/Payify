using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.SubMerchants;

public class SubMerchantDocumentDto : IMapFrom<SubMerchantDocument>
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid SubMerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
