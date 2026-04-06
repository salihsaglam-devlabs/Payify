namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class TopupCancelResponse
{
    public Guid CardTopupRequestId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}