namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankInstitutionListResponse
{
    public string institutionOid { get; set; }
    public string institutionCode { get; set; }
    public string institutionName { get; set; }
    public string sectorName { get; set; }
    public string customerCode { get; set; }
    public string operationMode { get; set; }
    public string subscriberNoRequirement { get; set; }
    public List<SekerBankField> subscriberNoDetails { get; set; }
}