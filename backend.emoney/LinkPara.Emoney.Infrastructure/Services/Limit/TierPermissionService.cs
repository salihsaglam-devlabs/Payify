using AutoMapper;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionsQuery;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class TierPermissionService : ITierPermissionService
{
    private readonly IGenericRepository<TierPermission> _tierPermissionRepository;
    private readonly IMapper _mapper;
    
    public TierPermissionService(
        IGenericRepository<TierPermission> tierPermissionRepository,
        IMapper mapper)
    {
        _tierPermissionRepository = tierPermissionRepository;
        _mapper = mapper;
    }
    
    public async Task<List<TierPermissionDto>> GetTierPermissionsQueryAsync(GetTierPermissionsQuery request)
    {
        var query = _tierPermissionRepository
            .GetAll().Where(tp => tp.RecordStatus == RecordStatus.Active);

        if (request.TierLevel.HasValue)
        {
            query = query.Where(tp => tp.TierLevel == request.TierLevel);
        }

        if (request.PermissionType.HasValue)
        {
            query = query.Where(tp => tp.PermissionType == request.PermissionType);
        }
        
        if (request.IsEnabled.HasValue)
        {
            query = query.Where(tp => tp.IsEnabled == request.IsEnabled);
        }
        
        return _mapper.Map<List<TierPermissionDto>>(await query.ToListAsync());
    }
    
    private async Task<TierPermission> GetPermissionAsync(AccountKycLevel accountKycLevel, TierPermissionType tierPermissionType)
    {
        return await _tierPermissionRepository.GetAll()
            .Where(tp =>
                tp.TierLevel == GetTierLevelType(accountKycLevel) &&
                tp.PermissionType == tierPermissionType)
            .SingleOrDefaultAsync();
    }
    
    private static TierLevelType GetTierLevelType(AccountKycLevel accountKycLevel)
    {
        return accountKycLevel switch
        {
            AccountKycLevel.NoneKyc => TierLevelType.Tier0,
            AccountKycLevel.PreKyc => TierLevelType.Tier1,
            AccountKycLevel.Kyc => TierLevelType.Tier2,
            AccountKycLevel.Premium => TierLevelType.Tier3,
            AccountKycLevel.PremiumPlus => TierLevelType.Tier4,
            AccountKycLevel.ChildKyc => TierLevelType.Tier5,
            AccountKycLevel.CorporateKyc => TierLevelType.Corporate,
            _ => TierLevelType.Custom
        };
    }

    public async Task ValidatePermissionAsync(AccountKycLevel kycLevel, TierPermissionType permissionType)
    {
        var permission = await GetPermissionAsync(kycLevel, permissionType);
        
        if (permission is not null && !permission.IsEnabled)
        {
            throw new InvalidTierPermissionException();
        }
    }
}