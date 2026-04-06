using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class SourceBankAccountDto
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }

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
}