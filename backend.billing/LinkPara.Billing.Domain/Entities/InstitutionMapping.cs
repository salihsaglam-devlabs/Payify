using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class InstitutionMapping : AuditEntity
{
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public string Customer { get; set; }
    public string VendorInstitutionId { get; set; }
}