namespace LinkPara.Emoney.Application.Features.OnUsPayments;

public class OnUsPaymentResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string OnUsPaymentRequestId { get; set; }
}
