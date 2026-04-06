namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class PaymentContractDto
{
    public PaymentConsentDto RzBlg { get; set; }
    public PaymentParticipantDto ParticipantInfo { get; set; }
    public PaymentGkdDto Gkd { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }
    public PaymentCompanyDto IsyOdmBlg { get; set; }
    public PaymentOrderInfoDto EmrBlg { get; set; }
}

