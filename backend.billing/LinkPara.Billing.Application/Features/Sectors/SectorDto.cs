using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Features.Sectors;

public class SectorDto : IMapFrom<Sector>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
