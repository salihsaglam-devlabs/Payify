using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.MerchantContents;

namespace LinkPara.PF.Application.Features.LinkPayments;

public class LinkPaymentPageResponse
{
    public LinkDto Link { get; set; }
    public List<MerchantContentDto> LinkContents { get; set; }
    public MerchantLogoDto LinkLogo { get; set; }
}
