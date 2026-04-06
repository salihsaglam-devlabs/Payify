namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class PaymentOrderDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public PaymentOrderResponseDto Result { get; set; }
}