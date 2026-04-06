namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class ActiveAccountConsentDto
{
    public string ApplicationUser { get; set; }
    public int Total { get; set; }
    public string CustomerTotalTlAmount { get; set; }
    public string CustomerTotalBlockedTlAmount { get; set; }
    public List<AccountConsent> AccountConsents { get; set; }
    
}

public class AccountConsent
{
    public string Bank { get; set; }
    public string BankCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime AccessTokenEndDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ConsentId { get; set; }
    public string BankTotalTlAmount { get; set; }
    public string BankTotalBlockedTlAmount { get; set; }
    public List<string> ConsentType { get; set; }
    public List<YosAccount> Accounts { get; set; }
    public List<CardsResponseDto> CardList { get; set; }

}

public class YosAccount
{
    public string Name { get; set; }
    public string Iban { get; set; }
    public string Href { get; set; }
    public string HspShb { get; set; }
    public DateTime OpenDate { get; set; }
    public string HspTur { get; set; }
    public string HspTip { get; set; }
    public string Currency { get; set; }
    public string TlAmount { get; set; }
    public string BlockedTlAmount { get; set; }
    public string KmhTlAmount { get; set; }
    public string Branch { get; set; }
    public string Balance { get; set; }
    public DateTime UpdateDate { get; set; }
    public string BlkTtr { get; set; }
    public YosCreditAccount KrdHsp { get; set; }

}

public class YosCreditAccount
{
    public string KulKrdTtr { get; set; }
    public string KrdDhlGstr { get; set; }

}
