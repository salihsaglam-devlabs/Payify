using LinkPara.Emoney.Application.Features.AccountServiceProviders;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class TriggerFuturePaymentOrderResponseDto : TriggerFuturePaymentOrderResultDto
{
    public ConsentInformation RzBlg { get; set; }
    public AccountGkdInformation Gkd { get; set; }
    public PaymentParticipantDto KatilimciBlg { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }
}

