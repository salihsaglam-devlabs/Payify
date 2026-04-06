using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Application.Features.Brands;

public class BrandDto : IMapFrom<Brand>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public Guid PublisherId { get; set; }
}
