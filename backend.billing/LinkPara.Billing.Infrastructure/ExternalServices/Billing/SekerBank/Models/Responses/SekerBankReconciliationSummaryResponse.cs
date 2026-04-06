namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankReconciliationSummaryResponse
{
    public string reconciliationStatus { get; set; }
    public string reconciliationDate { get; set; }
    public decimal totalPaymentAmount { get; set; }
    public decimal totalCancelationAmount { get; set; }
    public int totalPaymentCount { get; set; }
    public int totalCancelationCount { get; set; }
}