namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Responses;

public class PaycoreGetCustomerInfoResponse
{
    public Cstcustomeridentity cstCustomerIdentity { get; set; }
    public Stmtaccountstat stmtAccountStat { get; set; }
    public Crdaccountlastactivity crdAccountLastActivity { get; set; }
    public Cstcustomeraddress[] cstCustomerAddresses { get; set; }
    public Cstcustomercommunication[] cstCustomerCommunications { get; set; }
    public Customerlimit[] customerLimits { get; set; }
    public string primaryCardNo { get; set; }
    public string statCode { get; set; }
    public string bankingCustomerNo { get; set; }
    public string customerNo { get; set; }
    public string name { get; set; }
    public string midname { get; set; }
    public string surname { get; set; }
    public string commLanguage { get; set; }
    public string riskCode { get; set; }
    public string customerGroupCode { get; set; }
    public bool? isGuaranteed { get; set; }
    public bool? isBusiness { get; set; }
    public bool? isIdentityPresented { get; set; }
    public string companyNo { get; set; }
    public string companyName { get; set; }
    public string title { get; set; }
    public string workPlace { get; set; }
    public string graduation { get; set; }
    public string profession { get; set; }
    public string disabledType { get; set; }
    public string companyEmbossName { get; set; }
    public bool? hasCar { get; set; }
    public bool? hasRealEstate { get; set; }
    public string guarantor { get; set; }
    public string guarantorProfession { get; set; }
    public DateTime? statChangeDate { get; set; }
    public DateTime? lastCardIssuingDate { get; set; }
    public DateTime? firstCreditCardDate { get; set; }
    public int? branchCode { get; set; }
    public string customerEmbossNameExt { get; set; }
    public bool? isAllowedShareCstInfo { get; set; }
    public DateTime? shareCstInfoChgDate { get; set; }
    public int? digitalSlipType { get; set; }
}
public class Cstcustomeridentity
{
    public string bankingCustomerNo { get; set; }
    public string birthCityCode { get; set; }
    public string identitySerialNo { get; set; }
    public string identityIssuedBy { get; set; }
    public DateTime? passportDateOfIssue { get; set; }
    public DateTime? passportDateOfExpiry { get; set; }
    public string nationality { get; set; }
    public DateTime? birthDate { get; set; }
    public string birthPlace { get; set; }
    public string birthCountryCode { get; set; }
    public string gender { get; set; }
    public string fatherName { get; set; }
    public string motherName { get; set; }
    public string maidenName { get; set; }
    public string motherMaidenName { get; set; }
    public string maritalCode { get; set; }
    public string identityType { get; set; }
    public string nationalIdentityNo { get; set; }
    public string identityNo { get; set; }
    public DateTime? identityValidUntil { get; set; }
    public DateTime? identityIssueDate { get; set; }
    public string identityVillage { get; set; }
    public string identityFamilyNumber { get; set; }
    public string identityOrderNo { get; set; }
    public string identityCoverNo { get; set; }
    public string identityRegisterNo { get; set; }
    public string passportNo { get; set; }
    public string passportSerialNo { get; set; }
    public string passportIssuedBy { get; set; }
    public string passportIssuingCountryCode { get; set; }
    public string identityTownCode { get; set; }
    public string identityCityCode { get; set; }
    public string taxNo { get; set; }
    public string taxDepartmentName { get; set; }
    public string partnerName { get; set; }
}

public class Stmtaccountstat
{
    public DateTime? firstDelayDate { get; set; }
    public string followUpStat { get; set; }
    public string stmtStatCode { get; set; }
    public int? stmtDelinqPeriod { get; set; }
    public int? nplCount { get; set; }
    public int? minPayCount { get; set; }
    public int? prevMinPayCount { get; set; }
    public DateTime? minPayChangeDate { get; set; }
    public int? minPayDelinq { get; set; }
}

public class Crdaccountlastactivity
{
    public DateTime? lastTxnDate { get; set; }
    public string activityStat { get; set; }
    public int? activityStatCount { get; set; }
}

public class Cstcustomeraddress
{
    public int? idx { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public bool? isDefault { get; set; }
    public string addressType { get; set; }
    public string townCode { get; set; }
    public string cityCode { get; set; }
    public string countryCode { get; set; }
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string address3 { get; set; }
    public string address4 { get; set; }
    public string district { get; set; }
    public string zipCode { get; set; }
    public string addressCode { get; set; }
}

public class Cstcustomercommunication
{
    public int? idx { get; set; }
    public string communicationType { get; set; }
    public string info { get; set; }
    public bool? isDefault { get; set; }
}

public class Customerlimit
{
    public int? currentLimit { get; set; }
    public bool? isLimitBlocked { get; set; }
    public bool? isAutoLimitIncrease { get; set; }
    public string limitAssignType { get; set; }
    public int? currencyCode { get; set; }
}
