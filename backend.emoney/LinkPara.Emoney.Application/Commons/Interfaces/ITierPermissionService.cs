using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionsQuery;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ITierPermissionService
{
    Task<List<TierPermissionDto>> GetTierPermissionsQueryAsync(GetTierPermissionsQuery request);
    Task ValidatePermissionAsync(AccountKycLevel kycLevel, TierPermissionType permissionType);
}