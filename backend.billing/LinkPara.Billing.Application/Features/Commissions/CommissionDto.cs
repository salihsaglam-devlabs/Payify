using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Application.Features.Commissions;

public class CommissionDto : IMapFrom<Commission>
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; }
    public PaymentSource PaymentType { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string SectorName { get; set; }
    public RecordStatus RecordStatus { get; set; }
}