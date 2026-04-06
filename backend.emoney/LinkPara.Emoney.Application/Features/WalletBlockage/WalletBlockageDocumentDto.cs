using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.WalletBlockages;

public class WalletBlockageDocumentDto : IMapFrom<WalletBlockageDocument>
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}
