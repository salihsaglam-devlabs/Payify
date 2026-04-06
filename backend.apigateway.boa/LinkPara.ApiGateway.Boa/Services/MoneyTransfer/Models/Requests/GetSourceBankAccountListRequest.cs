using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Requests;

public class GetSourceBankAccountListRequest : SearchQueryParams
{
    public string CurrencyCode { get; set; } 
    public int BankCode { get; set; }
}

public class GetSourceBankAccountListServiceRequest : GetSourceBankAccountListRequest
{
    public TransactionSource? Source { get; set; } = TransactionSource.Emoney;
    public BankAccountType? AccountType { get; set; } = BankAccountType.UsageAccount;
    public RecordStatus? RecordStatus { get; set; } = SharedModels.Persistence.RecordStatus.Active;
}