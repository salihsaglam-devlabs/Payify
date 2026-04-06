using LinkPara.Emoney.Application.Features.Banks;

namespace LinkPara.Emoney.Application.Features.SystemBankAccounts;

public class SystemBankAccountDto
{
    public string Iban { get; set; }
    public string Name { get; set; }
    public virtual BankDto Bank { get; set; }

}