using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IAuditLogService _auditLogService;

    public PermissionService(IRepository<Permission> permissionRepository,
        IAuditLogService auditLogService)
    {
        _permissionRepository = permissionRepository;
        _auditLogService = auditLogService;
    }

    public async Task AddRangePermissionAsync(List<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            permission.NormalizedClaimValue = permission.ClaimValue.ToUpperInvariant();

            if (!_permissionRepository.GetAll().Any(q => q.NormalizedClaimValue == permission.NormalizedClaimValue &&
                                                         q.ClaimType == permission.ClaimType))
            {
                await _permissionRepository.AddAsync(permission);

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "AddRangePermission",
                    SourceApplication = "Identity",
                    Resource = "Permission",
                    Details = new Dictionary<string, string>
                    {
                          {"Id", permission.Id.ToString() },
                          {"ClaimValue", permission.ClaimValue.ToString() }
                    }
                });
            }
        }
    }

    public async Task UpdatePermissionAsync(Permission permission)
    {
        await _permissionRepository.UpdateAsync(permission);
    }

    public async Task<List<PermissionDto>> GetAllPermissionAsync()
    {
        return await _permissionRepository.GetAll().Where(q => q.Display)
            .GroupBy(x => new { x.Module, x.DisplayName }, (key, group) => new PermissionDto
            {
                Module = key.Module,
                DisplayName = key.DisplayName,
                ClaimByOperationTypes = group.Select(g =>
                    new ClaimByOperationType
                    {
                        Id = g.Id,
                        ClaimValue = g.ClaimValue,
                        OperationType = g.OperationType,
                        Description = g.Description
                    }).OrderBy(o => o.OperationType).ToList()
            }).OrderBy(o => o.DisplayName).ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionOwnerModulesByIdAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);

        var ownerModuleList = await _permissionRepository.GetAll().Where(q => q.Module == permission.Module).ToListAsync();

        return ownerModuleList;
    }

    public async Task<Permission> GetPermissionByIdAsync(Guid id)
    {
        return await _permissionRepository.GetByIdAsync(id);
    }

    public Task<bool> CheckInPermissionsAsync(string claimValue)
    {
        return Task.FromResult(_permissionRepository.GetAll()
            .Any(q => q.NormalizedClaimValue == claimValue.ToUpperInvariant()));
    }
}