namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;

public class UpdateCustomerRequest
{
    public string BankingCustomerNo { get; set; }
    public string CustomerGroupCode { get; set; }
    public string Name { get; set; }
    public string Midname { get; set; }
    public string Surname { get; set; }
    public string CustomerNo { get; set; }
    public string StatCode { get; set; }
    public string StatCodeFreeText { get; set; }
    public string RiskCode { get; set; }
    public int BranchCode { get; set; }
    public string CommLanguage { get; set; }
    public bool IsGuaranteed { get; set; }
    public bool IsBusiness { get; set; }
    public string CompanyNo { get; set; }
    public string CompanyName { get; set; }
    public string Graduation { get; set; }
    public string Profession { get; set; }
    public string CompanyEmbossName { get; set; }
    public string CustomerEmbossNameExt { get; set; }
    public string DisabledType { get; set; }
    public bool IsAllowedShareCstInfo { get; set; }
    public DateTime ShareCstInfoChgDate { get; set; }
    public string Title { get; set; }
    public string WorkPlace { get; set; }
    public bool HasCar { get; set; }
    public bool HasRealEstate { get; set; }
    public string Guarantor { get; set; }
    public string GuarantorProfession { get; set; }
    public string PrimaryCardNo { get; set; }
    public bool IsIdentityPresented { get; set; }
    public int DigitalSlipType { get; set; }
    public CstAccount[] CstAccounts { get; set; }
}

public class CstAccount
{
    public CstAccountLimit[] CstAccountLimits { get; set; }
}

public class CstAccountLimit
{
    public int CurrencyCode { get; set; }
    public int CurrentLimit { get; set; }
    public int LimitRatio { get; set; }
    public string UsageType { get; set; }
    public DateTime LcsSectorFirstUsedDate { get; set; }
    public bool IsOverlimitAllowed { get; set; }
    public string LimitAssignType { get; set; }
    public string ResetType { get; set; }
    public int ResetPeriod { get; set; }
    public CrdAccountLimitExt[] CrdAccountLimitExts { get; set; }
}

public class CrdAccountLimitExt
{
    public int CurrencyCode { get; set; }
    public int CurrentLimit { get; set; }
    public int LimitRatio { get; set; }
    public string EffectType { get; set; }
    public string UsageType { get; set; }
}
