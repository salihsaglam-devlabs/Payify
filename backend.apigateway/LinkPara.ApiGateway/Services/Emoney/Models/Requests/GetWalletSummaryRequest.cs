namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetWalletSummaryRequest
{
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public string PhoneCode { get; set; }
}
