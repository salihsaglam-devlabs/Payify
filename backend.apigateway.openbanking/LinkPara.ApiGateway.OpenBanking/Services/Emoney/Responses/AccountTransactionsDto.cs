namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class AccountTransactionsDto
{
    public string TotalRecord { get; set; }
    public List<AccTranDto> TransactionList { get; set; }
}

