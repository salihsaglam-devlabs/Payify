using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class SavedBill : AuditEntity
{
    public Guid UserId { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
}