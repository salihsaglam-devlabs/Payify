namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class TopupPreviewRequest
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string Currency { get; set; }
}