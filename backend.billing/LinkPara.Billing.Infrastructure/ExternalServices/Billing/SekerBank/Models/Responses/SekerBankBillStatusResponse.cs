namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankBillStatusResponse : SekerBankBillingTransaction
{
    public string bpcOid { get; set; }
    public string bpcDebtOid { get; set; }
    public string billStatus { get; set; }
}