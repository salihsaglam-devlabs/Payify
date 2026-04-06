using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Application.Features.Vendors;

public class VendorDto : IMapFrom<Vendor>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RecordStatus RecordStatus { get; set; }
}