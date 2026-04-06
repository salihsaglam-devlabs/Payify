namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class UserAccountResultDto
{
    public List<AccountDetail> AccountDetailList { get; set; }
}

public class AccountDetail
{
    public string AccountName { get; set; }
    public string IbanNo { get; set; }
    public string Fec { get; set; }
    public string CustomerName { get; set; }
    public string AvailableBalance { get; set; }
}