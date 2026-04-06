using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class TransactionStatistics
{
    public Guid VendorId { get; set; }
    public Guid InstitutionId { get; set; }
    public decimal PaymentAmount { get; set; }
    public int PaymentCount { get; set; }
    public decimal CancellationAmount { get; set; }
    public int CancellationCount { get; set; }
    public DateTime StatisticsDate { get; set; }
}