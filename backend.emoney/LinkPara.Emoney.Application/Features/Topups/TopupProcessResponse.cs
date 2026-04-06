namespace LinkPara.Emoney.Application.Features.Topups;

public class TopupProcessResponse
{
    public Guid CardTopupRequestId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
}