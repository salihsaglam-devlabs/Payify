using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class WalletBalanceResponse
{
    public decimal TotalBalance { get; set; }
    public PaginatedList<WalletBalanceDto> WalletBalances { get; set; }
}
