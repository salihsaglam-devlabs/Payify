using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.Chargebacks;

public class ChargebackDocumentDto : IMapFrom<ChargebackDocument>
{
    public Guid Id { get; set; }
    public Guid ChargebackId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}
