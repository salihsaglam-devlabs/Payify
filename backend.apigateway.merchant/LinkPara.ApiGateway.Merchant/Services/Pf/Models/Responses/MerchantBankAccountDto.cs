namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantBankAccountDto
{
    public Guid Id { get; set; }
    public string Iban { get; set; }
    public int BankCode { get; set; }
}
