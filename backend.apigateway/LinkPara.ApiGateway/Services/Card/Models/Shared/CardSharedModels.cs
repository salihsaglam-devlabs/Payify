namespace LinkPara.ApiGateway.Services.Card.Models.Shared;

public class CardAccount
{
    public AccountCommunication CrdAccountCommunication { get; set; }
}

public class AccountCommunication
{
    public string MobilePhone { get; set; }
    public string Email { get; set; }
}

public class CardDelivery
{
    public PaycoreAddress CardPostAddress { get; set; }
}

public class PaycoreAddress
{
    public string AddressType { get; set; }
    public string TownCode { get; set; }
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public string Address1 { get; set; }
}

public class CrdCardAuth
{
    public bool authDomEcom { get; set; }
    public bool authDomMoto { get; set; }
    public bool authDomNoCVV2 { get; set; }
    public bool authDomContactless { get; set; }
    public bool authDomCash { get; set; }
    public bool authDomCasino { get; set; }
    public bool authIntEcom { get; set; }
    public bool authIntMoto { get; set; }
    public bool authIntNoCVV2 { get; set; }
    public bool authIntContactless { get; set; }
    public bool authIntCash { get; set; }
    public bool authIntCasino { get; set; }
    public bool authIntPosSale { get; set; }
    public bool auth3dSecure { get; set; }
    public string auth3dSecureType { get; set; }
    public bool authCloseInstallment { get; set; }
    public bool visaAccountUpdaterOpt { get; set; }
}

public class AdditionalLimit
{
    public int CurrencyCode { get; set; }
    public int CurrentLimit { get; set; }
    public int LimitRatio { get; set; }
    public string EffectType { get; set; }
    public string UsageType { get; set; }
}

public class LimitRestriction
{
    public string StmtUsageType { get; set; }
    public int StmtMaxAmount { get; set; }
    public int StmtMaxCount { get; set; }
    public int StmtMaxRatio { get; set; }
    public string TxnEffectType { get; set; }
    public int CurrencyCode { get; set; }
    public string EffectType { get; set; }
    public string TxnRegion { get; set; }
    public string TerminalType { get; set; }
    public string Description { get; set; }
    public string AtOnceUsageType { get; set; }
    public int MaxAmountAtOnce { get; set; }
    public int MaxRatioAtOnce { get; set; }
    public string DailyUsageType { get; set; }
    public int DailyMaxAmount { get; set; }
    public int DailyMaxCount { get; set; }
    public int DailyMaxRatio { get; set; }
    public string WeeklyUsageType { get; set; }
    public int WeeklyMaxAmount { get; set; }
    public int WeeklyMaxCount { get; set; }
    public int WeeklyMaxRatio { get; set; }
    public string MonthlyUsageType { get; set; }
    public int MonthlyMaxAmount { get; set; }
    public int MonthlyMaxCount { get; set; }
    public int MonthlyMaxRatio { get; set; }
    public string YearlyUsageType { get; set; }
    public int YearlyMaxAmount { get; set; }
    public int YearlyMaxCount { get; set; }
    public int YearlyMaxRatio { get; set; }
}

public class CustomerAddress
{
    public int Idx { get; set; }
    public string City { get; set; }
    public string Town { get; set; }
    public bool IsDefault { get; set; }
    public string AddressType { get; set; }
    public string TownCode { get; set; }
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Address4 { get; set; }
    public string District { get; set; }
    public string ZipCode { get; set; }
    public string AddressCode { get; set; }
}

public class CustomerCommunication
{
    public int Idx { get; set; }
    public string CommunicationType { get; set; }
    public string Info { get; set; }
    public bool IsDefault { get; set; }
}

public class CustomerLimit
{
    public int CurrencyCode { get; set; }
    public float CurrentLimit { get; set; }
    public float LimitRatio { get; set; }
    public string UsageType { get; set; }
    public DateTime LcsSectorFirstUsedDate { get; set; }
    public bool IsOverlimitAllowed { get; set; }
    public string LimitAssignType { get; set; }
    public int ResetPeriod { get; set; }
}

public class CardTransactionResponseItem
{
    public string TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
}

public class CardLastCourierActivity
{
    public string CourierActivityCode { get; set; }
    public string CourierStatusDescription { get; set; }
    public DateTime CourierStatChangeDate { get; set; }
    public string CourierStatChangeTime { get; set; }
    public string CourierAddress { get; set; }
    public string CourierCity { get; set; }
    public string CardDeliveryRecipientName { get; set; }
    public string Brand { get; set; }
    public int CourierCompanyCode { get; set; }
    public string CourierCompanyName { get; set; }
    public string CardNo { get; set; }
    public int BranchCode { get; set; }
    public DateTime EmbossDate { get; set; }
    public string EmbossName1 { get; set; }
    public string CompanyName { get; set; }
}

public class PaycoreProduct
{
    public long guid { get; set; }
    public string dci { get; set; }
    public string code { get; set; }
    public string description { get; set; }
    public int expiryPeriodMonths { get; set; }
    public string segment { get; set; }
    public string physicalType { get; set; }
    public string plasticType { get; set; }
    public object virtualCardProductCode { get; set; }
    public object hceCardProductCode { get; set; }
    public object[] customerGroups { get; set; }
    public bool isBusiness { get; set; }
    public bool isEmv { get; set; }
    public bool isContactless { get; set; }
    public bool isOpen { get; set; }
    public string expiryDateType { get; set; }
    public bool? hasPhoto { get; set; }
    public object bin { get; set; }
    public object isBrandShared { get; set; }
}