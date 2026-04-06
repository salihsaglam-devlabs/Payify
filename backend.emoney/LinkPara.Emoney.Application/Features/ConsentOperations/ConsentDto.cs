namespace LinkPara.Emoney.Application.Features.ConsentOperations;

public class ConsentDto
{
    public string TppName { get; set; }
    public string ConsentId { get; set; }
    public string CreateDate { get; set; }
    public DateTime CreateDateValue { get; set; }
    public string Status { get; set; }
    public string StatusDetay { get; set; }
    public string LastAccessDate { get; set; }
    public DateTime LastAccessDateValue { get; set; }
    public List<AccountConsentDto> Accounts { get; set; }
    public string Iban { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

}