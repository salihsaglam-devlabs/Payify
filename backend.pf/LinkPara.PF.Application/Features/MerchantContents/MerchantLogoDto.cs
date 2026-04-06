using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.MerchantContents;

public class MerchantLogoDto : IMapFrom<MerchantLogo>
{
    public Guid MerchantId { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
}