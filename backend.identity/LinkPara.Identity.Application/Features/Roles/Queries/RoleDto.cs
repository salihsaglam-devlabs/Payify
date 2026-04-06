using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Roles.Queries;

public class RoleDto : IMapFrom<Role>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public RecordStatus RecordStatus { get; set; }
}