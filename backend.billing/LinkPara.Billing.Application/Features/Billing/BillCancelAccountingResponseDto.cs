namespace LinkPara.Billing.Application.Features.Billing;

public class BillCancelAccountingResponseDto
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ReferenceNumber { get; set; }
}