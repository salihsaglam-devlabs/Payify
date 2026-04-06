namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankBillInquiryResponse
{
    public string requestId { get; set; }
    public string institutionOid { get; set; }
    public string institutionCode { get; set; }
    public string institutionShortName { get; set; }
    public List<SekerBankDebt> debts { get; set; }
}