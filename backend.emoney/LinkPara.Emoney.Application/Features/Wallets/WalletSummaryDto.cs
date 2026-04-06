using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Wallets;

public class WalletSummaryDto : IMapFrom<Wallet>
{
    public string WalletNumber { get; set; }
    public string UserName { get; set; }
    public decimal Balance { get; set; }
    public string CurrencySymbol { get; set; }
}
