using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class SectorMapping : AuditEntity
{
    public Guid SectorId { get; set; }
    public Sector Sector { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public string VendorSectorId { get; set; }
}