namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;
public class CreateStandingPaymentOrderConsentRequest
{
    public string ConsentId { get; set; }
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string YonTipi { get; set; }
    public PaymentGkdDto Gkd { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }
    public PaymentDetail OdmAyr { get; set; }
    
}

