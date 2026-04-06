namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class TriggerFuturePaymentOrderResponseDto : TriggerFuturePaymentOrderResultDto
{
    public ConsentInformation RzBlg { get; set; }
    public AccountGkdInformation Gkd { get; set; }
    public PaymentParticipantDto KatilimciBlg { get; set; }
    public PaymentInformationResponseDto OdmBsltm { get; set; }
}

public class PaymentParticipantDto
{
    public string HhsCode { get; set; }
    public string YosCode { get; set; }
}

