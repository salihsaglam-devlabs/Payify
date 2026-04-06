using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class ReconciliationDetail : AuditEntity
{
    public Guid ReconciliationSummaryId { get; set; }
    public ReconciliationSummary ReconciliationSummary { get; set; }
    public ReconciliationDetailStatus ReconciliationDetailStatus { get; set; }
    public int ExternalOrderId { get; set; }
    public decimal ExternalTotal { get; set; }
    public bool HasInternalOrders { get; set; }
    public bool HasExternalOrders { get; set; }
    public DateTime TransactionDate { get; set; }
    public string InternalOrderErrorMessage { get; set; }
    public Guid? OrderId { get; set; }
    public Order Order { get; set; }
    public Guid? OrderHistoryId { get; set; }
    public OrderHistory OrderHistory { get; set; }
    public string ProductName { get; set; }
}
