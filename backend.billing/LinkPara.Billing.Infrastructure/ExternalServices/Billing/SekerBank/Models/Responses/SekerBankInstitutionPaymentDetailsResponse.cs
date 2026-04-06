namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankInstitutionPaymentDetailsResponse : SekerBankInstitutionReconciliation
{
    public List<SekerBankPayment> payments { get; set; }
    public List<SekerBankPayment> canceleds { get; set; }
}