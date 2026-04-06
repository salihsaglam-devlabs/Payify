using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class GetSourceBankAccountsRequest : SearchQueryParams
{
    public TransactionSource? Source { get; set; }
    public BankAccountType? AccountType { get; set; }
    public string CurrencyCode { get; set; }
    public int BankCode { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}