using LinkPara.Emoney.Application.Features.AccountServiceProviders;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class StandingPaymentOrderConsentResponseDto : StandingPaymentOrderConsentResultDto
{
    public ConsentInformation RzBlg { get; set; }
    
}

