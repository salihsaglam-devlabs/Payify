using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Roles.Queries;

public class RoleDetailDto : IMapFrom<Role>
{
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public List<string> Claims { get; set; }
    public RecordStatus RecordStatus { get; set; }
}