namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class PaymentOrderConsentDetailDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public PaymentConsentResultDto Result { get; set; }
}