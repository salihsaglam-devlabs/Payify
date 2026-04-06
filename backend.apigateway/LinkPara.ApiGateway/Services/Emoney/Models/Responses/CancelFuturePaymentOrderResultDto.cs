namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;
public class CancelFuturePaymentOrderResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public CancelFuturePaymentOrderResponseDto Result { get; set; }
}