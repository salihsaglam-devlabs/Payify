using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class WalletDto
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public string FriendlyName { get; set; }
    public Guid AccountId { get; set; }
    public string CurrencySymbol { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public bool IsMainWallet { get; set; }
    public WalletType WalletType { get; set; }
    public bool IsBlocked { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
}
