namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class BillInquiryResponseDto
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public BillInquiryResponse Response { get; set; }
}