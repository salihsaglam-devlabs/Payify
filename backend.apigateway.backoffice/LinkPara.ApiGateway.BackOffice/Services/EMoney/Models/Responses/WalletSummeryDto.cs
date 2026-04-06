namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class WalletSummeryDto
{
    public string WalletNumber { get; set; }
    public string UserName { get; set; }
    public decimal Balance { get; set; }
    public string CurrencySymbol { get; set; }
}