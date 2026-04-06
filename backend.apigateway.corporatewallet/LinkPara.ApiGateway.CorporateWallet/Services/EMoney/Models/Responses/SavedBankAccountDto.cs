namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class SavedBankAccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tag { get; set; }
    public string Iban { get; set; }
    public Guid BankId { get; set; }
    public BankDto Bank { get; set; }
}
