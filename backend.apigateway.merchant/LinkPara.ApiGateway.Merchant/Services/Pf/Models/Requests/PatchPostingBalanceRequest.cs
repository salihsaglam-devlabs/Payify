namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class PatchPostingBalanceRequest
{
    public int MoneyTransferBankCode { get; set; }
    public string MoneyTransferBankName { get; set; }
}
