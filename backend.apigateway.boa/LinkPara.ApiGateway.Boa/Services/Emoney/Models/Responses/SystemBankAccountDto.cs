namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class SystemBankAccountDto
{
    public string Iban { get; set; }
    public string Name { get; set; }
    public virtual EmoneyBankDto Bank { get; set; }
}