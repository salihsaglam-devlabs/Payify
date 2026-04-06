
namespace LinkPara.Emoney.Application.Features.AccountServiceProviders;

public class PaymentContractDto
{
    public PaymentContractDto()
    {
        
    }
    public PaymentContractDto(PaymentConsentDto rzBlg, 
        PaymentParticipantDto katilimciBlg, 
        PaymentGkdDto gkd, 
        PaymentInformationDto odmBsltm, 
        PaymentCompanyDto isyOdmBlg)
    {
        RzBlg = rzBlg;
        KatilimciBlg = katilimciBlg;
        Gkd = gkd;
        OdmBsltm = odmBsltm;
        IsyOdmBlg = isyOdmBlg;
    }

    public PaymentConsentDto RzBlg { get; set; }
    public PaymentParticipantDto KatilimciBlg { get; set; }
    public PaymentGkdDto Gkd { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }
    public PaymentCompanyDto IsyOdmBlg { get; set; }
    public PaymentOrderInfoDto EmrBlg { get; set; }
}


