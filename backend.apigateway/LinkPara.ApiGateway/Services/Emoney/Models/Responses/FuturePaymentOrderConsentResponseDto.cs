using LinkPara.ApiGateway.Services.Emoney.Models.Requests;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class FuturePaymentOrderConsentResponseDto : FuturePaymentOrderConsentResultDto
{
    public PaymentOrderInfoType EmrBlg { get; set; }
    public ConsentInformation RzBlg { get; set; }
    public PaymentInformationResponseDto OdmBsltm { get; set; }    
}

public class PaymentInformationResponseDto
{    
    public IdentityInfoDto Kmlk { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public ProcessAmountOb GrckIslTtr { get; set; }
    public RetailPersonOb Gon { get; set; }
    public RetailPersonOb Alc { get; set; }
    public QrCodeInfo Kkod { get; set; }
    public PaymentDetail OdmAyr { get; set; }
}

