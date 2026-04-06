using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class WalletBalanceDto
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public string FriendlyName { get; set; }
    public Guid? AccountId { get; set; }
    public string CurrencyCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<TransactionsDto> Transactions { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public decimal BlockedBalanceCredit { get; set; }
    public decimal AvailableBalance { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPartiallyBlocked { get; set; }
}
public class TransactionsDto
{
    public decimal CurrentBalance { get; set; }
    public DateTime TransactionDate { get; set; }
}