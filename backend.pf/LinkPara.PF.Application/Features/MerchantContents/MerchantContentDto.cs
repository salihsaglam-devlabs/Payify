using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantContents;

public class MerchantContentDto : IMapFrom<MerchantContent>
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public List<MerchantContentVersionDto> Contents { get; set; }
}