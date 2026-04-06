using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Application.Features.Roles.Commands.CreateRole;
using LinkPara.Identity.Application.Features.Roles.Commands.UpdateRole;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace LinkPara.Identity.Infrastructure.Services.Approval;

public class RoleScreenService : IApprovalScreenService
{

    private readonly IStringLocalizer _localizer;
    private readonly RoleManager<Role> _roleManager;
    private readonly IClaimService _claimService;

    public RoleScreenService(IStringLocalizerFactory factory,
        RoleManager<Role> roleManager,
        IClaimService claimService)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.Identity.API");
        _roleManager = roleManager;
        _claimService = claimService;
    }
    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        ///v1/Roles/894b23e6-985d-4eb5-89a0-08da9ff646bb
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var role = await _roleManager.FindByIdAsync(url.LastOrDefault());

        if (role is null)
        {
            throw new NotFoundException(nameof(Role), url.LastOrDefault());
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("RoleName").Value, role.Name}
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Roles"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreateRoleCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("RoleName").Value, requestBody.Name}
        };

        if (requestBody.Claims is not null)
        {
            data.Add(_localizer.GetString("Claims").Value, ClaimsToString(requestBody.Claims));
        }

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Roles"
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        ///v1/Roles/894b23e6-985d-4eb5-89a0-08da9ff646bb
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var role = await _roleManager.FindByIdAsync(url.LastOrDefault());

        if (role is null)
        {
            throw new NotFoundException(nameof(Role), url.LastOrDefault());
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("RoleName").Value, role.Name}
        };

        var requestBody = JsonConvert.DeserializeObject<UpdateRoleCommand>(request.Body);

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(role, requestBody, _localizer);

        var dbRoleClaims = await _claimService.GetRoleClaimsAsync(role);
        var dbModuleClaims = RoleClaimsToModuleClaims(dbRoleClaims);

        if(ClaimsToString(dbModuleClaims) != ClaimsToString(requestBody.Claims))
        {
            var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", ClaimsToString(dbModuleClaims) },
                        {"NewValue", ClaimsToString(requestBody.Claims) }
                    };
            updatedFields.Add(_localizer.GetString("Claims").Value, updatedField);

        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Roles",
            UpdatedFields = updatedFields
        };
    }

    private string ClaimsToString(ModuleClaim[] claims)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var claim in claims)
        {
            var permissions = claim.PermissionOperationTypes.Select(x => _localizer.GetString(x.ToString()).Value).ToList();
            if (permissions.Any())
            {
                sb.AppendLine($"{_localizer.GetString("Module").Value} : {_localizer.GetString(claim.Module).Value}, { _localizer.GetString("Permissons").Value} : { string.Join(',', permissions)}");
            }
        }
        return sb.ToString();
    }

    private ModuleClaim[] RoleClaimsToModuleClaims(IList<Claim> roleClaims) 
    {
        var result = new List<ModuleClaim>();
        foreach (var item in roleClaims)
        {
            var claimDetails = item.Value.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (claimDetails.Length != 2)
            {
                continue;
            }

            if (!result.Any(x => x.Module == claimDetails[0]))
            {
                result.Add(new ModuleClaim
                {
                    Module = claimDetails[0],
                    PermissionOperationTypes = new List<PermissionOperationType>
                    { 
                        (PermissionOperationType) Enum.Parse(typeof(PermissionOperationType), claimDetails[1]) 
                    }
                });
            }
            else
            {
                result.FirstOrDefault(x => x.Module == claimDetails[0])?.PermissionOperationTypes.Add((PermissionOperationType)Enum.Parse(typeof(PermissionOperationType), claimDetails[1])); 
            }
        }

        return result.ToArray();
    }
}
