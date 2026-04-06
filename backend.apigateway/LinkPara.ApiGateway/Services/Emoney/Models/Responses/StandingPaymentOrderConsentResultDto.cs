namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;
public class StandingPaymentOrderConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public StandingPaymentOrderConsentResponseDto Result { get; set; }
}