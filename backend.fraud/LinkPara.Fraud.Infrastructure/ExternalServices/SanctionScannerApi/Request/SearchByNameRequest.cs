namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Request;

public class SearchByNameRequest
{
    public string Name { get; set; }
    public int SearchType { get; set; } // Enum ?
    public int? Start { get; set; }
    public int? Limit { get; set; }
    public string ReferenceNumber { get; set; }
    public string OutReferenceNumber { get; set; }
    public string BirthYear { get; set; }
    public int? MinMatchRate { get; set; }
    public int? MaxMatchRate { get; set; }
    public bool? IsDeepSearch { get; set; }
    public string CountryCodes { get; set; }
    public string SearchProfileKey { get; set; }
}
