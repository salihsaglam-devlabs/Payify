using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class Institution : AuditEntity
{
    public string Name { get; set; }
    public Guid SectorId { get; set; }
    public Sector Sector { get; set; }
    public Guid ActiveVendorId { get; set; }
    public Vendor ActiveVendor { get; set; }
    public OperationMode OperationMode { get; set; }
    public FieldRequirementType FieldRequirementType { get; set; }
    public List<Field> Fields { get; set; }
}