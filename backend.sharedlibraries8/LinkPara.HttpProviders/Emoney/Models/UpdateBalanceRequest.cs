using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;
public class UpdateBalanceRequest
{
    public EmoneyTransactionType TransactionType { get; set; }
    public EmoneyTransactionDirection TransactionDirection { get; set; }
    public string Channel { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Utid { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public bool IsBalanceControl { get; set; }
}
