using LinkPara.ApiGateway.BackOffice.Commons.Mappings;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

public class UserDetailDto : UserDto, IMapFrom<UserDto>
{
    public List<UserDetailWalletDto> Wallets { get; set; }
}

public class UserDetailWalletDto : IMapFrom<WalletDto>
{
    public string WalletNumber { get; set; }
    public string FriendlyName { get; set; }
    public string CurrencySymbol { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public bool IsMainWallet { get; set; }
    public decimal AvailableBalance { get; set; }
    public WalletType WalletType { get; set; }
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
}