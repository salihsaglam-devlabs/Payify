using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Models.WalletModels;

public class WalletBalanceResponse
{
    public decimal TotalBalance { get; set; }
    public PaginatedList<WalletBalanceDto> WalletBalances { get; set; }
}
