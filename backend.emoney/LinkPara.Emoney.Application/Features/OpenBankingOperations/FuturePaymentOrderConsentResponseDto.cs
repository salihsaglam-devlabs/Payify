using LinkPara.Emoney.Application.Features.AccountServiceProviders;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class FuturePaymentOrderConsentResponseDto : FuturePaymentOrderConsentResultDto
{
    public PaymentOrderInfoType EmrBlg { get; set; }
    public ConsentInformation RzBlg { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }    
}

