namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;

public class BillCancelResponseDto
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    BillCancelResponse Response { get; set; }
}