using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class BankAccountBalanceRequest : SearchQueryParams
{
    public DateTime Date { get; set; }
    public int? BankCode { get; set; }
    public TransactionSource? Source { get; set; }
    public BankAccountType? AccountType { get; set; }
}
