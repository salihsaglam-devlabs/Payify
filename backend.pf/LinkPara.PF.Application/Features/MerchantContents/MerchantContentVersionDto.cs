using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.MerchantContents;

public class MerchantContentVersionDto : IMapFrom<MerchantContentVersion>
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string LanguageCode { get; set; }
}