using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Features.Limits;

public class TierPermissionDto : IMapFrom<TierPermission>
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public TierPermissionType PermissionType { get; set; }
    public bool IsEnabled { get; set; }
}