namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class WalletSummaryDto
{
    public string WalletNumber { get; set; }
    public string UserName { get; set; }
    public decimal Balance { get; set; }
    public string CurrencySymbol { get; set; }
}

