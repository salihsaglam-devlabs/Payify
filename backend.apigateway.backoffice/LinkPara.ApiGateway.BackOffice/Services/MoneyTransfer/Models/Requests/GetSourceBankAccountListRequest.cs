using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetSourceBankAccountListRequest : SearchQueryParams
{
    public TransactionSource? Source { get; set; }
    public BankAccountType? AccountType { get; set; }
    public string CurrencyCode { get; set; }
    public int BankCode { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
