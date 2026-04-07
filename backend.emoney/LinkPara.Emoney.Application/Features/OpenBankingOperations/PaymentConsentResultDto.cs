namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;


public class PaymentConsentResultDto
{
    public ConsentInformation RzBlg { get; set; }
    public AccountGkdInformation Gkd { get; set; }
    public StartPaymentConsentDto OdmBsltm { get; set; }
}

public class StartPaymentConsentDto
{
    public ParticipantInformation KatilimciBlg { get; set; }
    public AccountConsentIdentityInfo Kmlk { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public RetailPersonOb Gon { get; set; }
    public RetailPersonOb Alc { get; set; }
    public PaymentConsentResultDetailOb OdmAyr { get; set; }
    public PaymentConsentCompanyDto IsyOdmBlg { get; set; }
}

public class PaymentConsentResultDetailOb : PaymentConsentDetailOb
{
    public string OdmStm { get; set; }
    public string OhkMsj { get; set; }
    public DateTime BekOdmZmn { get; set; }
}