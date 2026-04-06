namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class TopupProcessResponse
{
    public Guid CardTopupRequestId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
}