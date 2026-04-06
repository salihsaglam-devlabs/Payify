namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class GetWalletListResponse
{
    public int TotalRecord { get; set; }
    public List<CustomerWaletDto> RecordList { get; set; }
}
