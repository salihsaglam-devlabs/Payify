using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class SaveSourceBankAccountRequest
{
    public TransactionSource Source { get; set; }
    public BankAccountType AccountType { get; set; }
    public int BankCode { get; set; }
    public string AccountName { get; set; }
    public string CompanyNumber { get; set; }
    public string AccountNumber { get; set; }
    public string AccountSuffix { get; set; }
    public string BranchCode { get; set; }
    public string CurrencyCode { get; set; }
    public string IBANNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
