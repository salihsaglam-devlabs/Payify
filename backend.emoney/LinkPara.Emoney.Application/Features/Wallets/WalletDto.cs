using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Wallets;

public class WalletDto : IMapFrom<Wallet>
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public string FriendlyName { get; set; }
    public Guid AccountId { get; set; }
    public string CurrencySymbol { get; set; }    
    public bool IsMainWallet { get; set; }   
    public WalletType WalletType { get; set; }
    public bool IsBlocked { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool P2PCreditBalanceUsable { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public decimal BlockedBalanceCredit { get; set; }
    public decimal AvailableBalanceCredit => CurrentBalanceCredit - BlockedBalanceCredit;
    public decimal AvailableBalanceCash => CurrentBalanceCash - BlockedBalance;
    public decimal AvailableBalance => AvailableBalanceCash + AvailableBalanceCredit;

    public static void Mapping(Profile profile)
    {
        profile.CreateMap<Wallet, WalletDto>().AfterMap((src, dest) =>
        {
            if (src is not null && src.Currency is not null)
            {
                dest.CurrencyCode = src.Currency.Code;
                dest.CurrencySymbol = src.Currency.Symbol;
            }
        });
    }
}
