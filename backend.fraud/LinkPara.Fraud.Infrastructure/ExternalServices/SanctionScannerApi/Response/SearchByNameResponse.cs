using Newtonsoft.Json;

namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Response;

public class SearchByNameResponse : BaseApiResponse
{
    public object ExtraInfo { get; set; }
    public Result Result { get; set; }
}

public class Result
{
    /// <summary>
    /// 0:Unknown
    ///1:No Match
    ///2:Potential Match
    ///3:False Positive
    ///4:True Positive
    ///5:True Positive Approve
    ///6:True Positive Reject
    /// </summary>
    public int MatchStatusId { get; set; }
    /// <summary>
    ///0:Unknown
    ///1:Low
    ///2:Medium
    ///3:High
    ///4:Neutral
    ///5:Increased
    ///6:Unacceptable
    /// </summary>
    public int RiskLevelId { get; set; }
    public object AssignedUserGuidId { get; set; }
    public object[] TagList { get; set; }
    public int TotalRecordCount { get; set; }
    public int ReturnRecordCount { get; set; }
    public int Start { get; set; }
    public int Limit { get; set; }
    public string ReferenceNumber { get; set; }
    public string NameSearchType { get; set; }
    public string OutReferenceNumber { get; set; }
    public bool IsWhiteList { get; set; }
    public object WhiteListMessage { get; set; }
    public int? MinMatchRate { get; set; }
    public int? MaxMatchRate { get; set; }
    public object BirthYear { get; set; }
    public bool IsSafeList { get; set; }
    public object NationalyFilter { get; set; }
    public bool IsZFS { get; set; }
    public int TotalProfileCount { get; set; }
    public int MatchRate { get; set; }
    [JsonProperty("Result")]
    public Result1[] Results { get; set; }
}

public class Result1
{
    public decimal MatchRate { get; set; }
    public bool IsMatchAka { get; set; }
    public string FirstName { get; set; }
    public object MiddleName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Nationality { get; set; }
    public object ListedOn { get; set; }
    public object LastUpdateDate { get; set; }
    public object ProgramKey { get; set; }
    public string ProgramName { get; set; }
    public object Position { get; set; }
    public object Country { get; set; }
    public object SanctionSet { get; set; }
    public object Origin { get; set; }
    public object Title { get; set; }
    public object Gender { get; set; }
    public object Function { get; set; }
    public object Language { get; set; }
    public string CitizenDetail { get; set; }
    public string CountryCode { get; set; }
    public object HonoricPrefix { get; set; }
    public object HonoricSuffix { get; set; }
    public object ContactDetails { get; set; }
    public object Email { get; set; }
    public object BirthDate { get; set; }
    public object DeathDate { get; set; }
    public string EntityType { get; set; }
    public string Type { get; set; }
    public object Remark { get; set; }
    public object FirstSeen { get; set; }
    public object LastSeen { get; set; }
    public object Summary { get; set; }
    public object FatherName { get; set; }
    public object Committees { get; set; }
    public string OtherInformation { get; set; }
    public object Basis { get; set; }
    public object BlackListTypeId { get; set; }
    public object RecordUniqueNumber { get; set; }
    public object Designation { get; set; }
    public object SubmittedBy { get; set; }
    public object Justification { get; set; }
    public object DataSource { get; set; }
    public object ListingInformation { get; set; }
    public string BlacklistName { get; set; }
    public string BlacklistFlagCode { get; set; }
    public object VesselCallSign { get; set; }
    public object VesselType { get; set; }
    public object VesselFlag { get; set; }
    public object VesselTonnage { get; set; }
    public object VesselGrossRegisteredTonnage { get; set; }
    public object VesselOwner { get; set; }
    public object RegisteredNumber { get; set; }
    public string ProfileKey { get; set; }
    public object PepClass { get; set; }
    public object PepClassId { get; set; }
    public object DeathYear { get; set; }
    public string ssid { get; set; }
    public Othername[] OtherNames { get; set; }
    public Document[] Documents { get; set; }
    public Birthdetail[] BirthDetails { get; set; }
    public Addressdetail[] AddressDetails { get; set; }
    public Image[] Images { get; set; }
    public object[] Links { get; set; }
    public object[] Modifications { get; set; }
    public object[] MemberShipData { get; set; }
    public object[] MediaData { get; set; }
}

public class Othername
{
    public string NameType { get; set; }
    public string FullName { get; set; }
    public object Language { get; set; }
    public object OtherInformation { get; set; }
    public object Quality { get; set; }
}

public class Document
{
    public string DocumentType { get; set; }
    public string DocumentTypeInformation { get; set; }
    public string DocumentNumber { get; set; }
    public string DocumentCountry { get; set; }
    public object DocumentCity { get; set; }
    public object IssueDate { get; set; }
    public object ExpirationDate { get; set; }
    public object OtherInformation { get; set; }
}

public class Birthdetail
{
    public string BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public object BirthCountry { get; set; }
    public object OtherInformation { get; set; }
    public object BirthYear { get; set; }
}

public class Addressdetail
{
    public object Number { get; set; }
    public object Street { get; set; }
    public object State { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public object IsoCode { get; set; }
    public string OtherInformation { get; set; }
}

public class Image
{
    public object DataSource { get; set; }
    public string Link { get; set; }
    public object OtherInformation { get; set; }
}



