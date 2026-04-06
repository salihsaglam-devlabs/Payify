namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public abstract class BaseTopupProcessRequest
{
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Guid UserId { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
