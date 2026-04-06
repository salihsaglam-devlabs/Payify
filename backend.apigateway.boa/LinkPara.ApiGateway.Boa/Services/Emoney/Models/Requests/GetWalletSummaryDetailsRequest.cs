namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetWalletSummaryDetailsRequest
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}
