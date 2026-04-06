using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class BankAccountBalanceDto
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string BankName { get; set; }
    public int BankCode { get; set; }
    public TransactionSource Source { get; set; }
    public BankAccountType AccountType { get; set; }
    public string IBANNumber { get; set; }
    public decimal CurrentBalance { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime Date { get; set; }
    public CheckBalanceStatus CheckBalanceStatus { get; set; }
}
