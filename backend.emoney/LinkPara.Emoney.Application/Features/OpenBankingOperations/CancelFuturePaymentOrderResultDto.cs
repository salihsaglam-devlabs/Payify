namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class CancelFuturePaymentOrderResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public CancelFuturePaymentOrderResponseDto Result { get; set; }
}