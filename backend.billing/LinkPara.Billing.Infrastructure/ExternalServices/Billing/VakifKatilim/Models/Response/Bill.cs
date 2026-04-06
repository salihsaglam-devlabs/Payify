namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Response;

public class Bill
{
    public int? InstitutionId { get; set; }
    public string SubscriberNumber { get; set; }
    public string BillNumber { get; set; }
    public string NameSurname { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? Amount { get; set; }
    public string SecondQueryField { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? InvoiceDelayAmount { get; set; }
    public string DebtProcessType { get; set; }
    public string DebtInfo { get; set; }
    public string AdditionalInfo { get; set; }
    public int? Priority { get; set; }
    public string Description { get; set; }
}