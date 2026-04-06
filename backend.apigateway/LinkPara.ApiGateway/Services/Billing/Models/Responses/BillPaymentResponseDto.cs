namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class BillPaymentResponseDto
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public BillPaymentResponse Response { get; set; }
}