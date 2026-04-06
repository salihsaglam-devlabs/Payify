namespace LinkPara.Emoney.Application.Commons.Models.ReceiptModels;

public sealed class ReceiptParty
{
    public string WalletNumber { get; set; }
    public string Iban { get; set; }
    public string FullName { get; set; }
    public string MaskedCardNumber { get; set; }
    public string BankName { get; set; }
}
