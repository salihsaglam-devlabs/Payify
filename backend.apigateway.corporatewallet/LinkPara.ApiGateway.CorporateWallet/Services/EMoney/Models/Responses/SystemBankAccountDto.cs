namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class SystemBankAccountDto
{
    public string Iban { get; set; }
    public string Name { get; set; }
    public virtual BankDto Bank { get; set; }
}