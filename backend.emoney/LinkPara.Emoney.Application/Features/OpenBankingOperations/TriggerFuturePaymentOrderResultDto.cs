namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class TriggerFuturePaymentOrderResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public TriggerFuturePaymentOrderResponseDto Result { get; set; }
}