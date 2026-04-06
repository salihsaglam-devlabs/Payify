namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class GetWalletBalanceListResponse
{
    public int TotalRecord { get; set; }
    public List<CustomerWalletBalanceDto> RecordList { get; set; }
}
