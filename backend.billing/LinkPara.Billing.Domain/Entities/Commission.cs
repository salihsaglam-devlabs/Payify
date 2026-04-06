using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class Commission : AuditEntity
{
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public PaymentSource PaymentType { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
}