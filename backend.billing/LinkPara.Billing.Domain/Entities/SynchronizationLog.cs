using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class SynchronizationLog : AuditEntity
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; }
    public Guid VendorId { get; set; }
    public SynchronizationItem SynchronizationItem { get; set; }
    public SynchronizationType SynchronizationType { get; set; }
    public DateTime SynchronizationDate { get; set; }
}