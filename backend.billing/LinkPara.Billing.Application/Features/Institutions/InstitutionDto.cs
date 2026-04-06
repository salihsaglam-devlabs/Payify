using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Application.Features.Institutions;

public class InstitutionDto : IMapFrom<Institution>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; }
    public Sector Sector { get; set; }
    public Guid ActiveVendorId { get; set; }
    public string ActiveVendorName { get; set; }
    public OperationMode OperationMode { get; set; }
    public FieldRequirementType FieldRequirementType { get; set; }
    public List<Field> Fields { get; set; }
    public RecordStatus RecordStatus { get; set; }
}