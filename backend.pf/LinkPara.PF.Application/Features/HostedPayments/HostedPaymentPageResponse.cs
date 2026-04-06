using LinkPara.PF.Application.Features.MerchantContents;

namespace LinkPara.PF.Application.Features.HostedPayments;

public class HostedPaymentPageResponse
{
    public HostedPaymentDto HostedPayment { get; set; }
    public MerchantLogoDto MerchantLogo { get; set; }
    public List<MerchantContentDto> MerchantContents { get; set; }
}