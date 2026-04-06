using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetAccountBanksRequest
{
    public TransactionSource? Source { get; set; }
    public BankAccountType? AccountType { get; set; }
}
