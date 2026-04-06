namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class FuturePaymentOrderConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public FuturePaymentOrderConsentResponseDto Result { get; set; }
}