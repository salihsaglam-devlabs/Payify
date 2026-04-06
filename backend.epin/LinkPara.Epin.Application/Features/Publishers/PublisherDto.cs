using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Application.Features.Publishers;

public class PublisherDto : IMapFrom<Publisher>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
