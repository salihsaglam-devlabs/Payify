using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Wallets;

public class WalletBalanceDto : IMapFrom<Wallet>
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public string FriendlyName { get; set; }
    public Guid? AccountId { get; set; }
    public string CurrencyCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<Transaction> Transactions { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public decimal BlockedBalanceCredit { get; set; }
    public decimal AvailableBalance { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPartiallyBlocked => (!IsBlocked && (BlockedBalance > 0 || CurrentBalanceCredit > 0));

}

