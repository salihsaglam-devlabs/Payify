using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantBlockages;

public class MerchantBlockageDetailDto : IMapFrom<MerchantBlockageDetail>
{
    public Guid Id { get; set; }
    public DateTime PostingDate { get; set; }
    public Guid MerchantId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
}
