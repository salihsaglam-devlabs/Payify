using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQuery : IRequest<RoleDetailDto>
{
    public Guid RoleId { get; set; }
}

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailDto>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IClaimService _claimService;

    public GetRoleByIdQueryHandler(RoleManager<Role> roleManager,
        IClaimService claimService)
    {
        _roleManager = roleManager;
        _claimService = claimService;
    }

    public async Task<RoleDetailDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), request.RoleId);
        }

        var resultRoleDetail = new RoleDetailDto() {
            Name = role.Name, 
            RoleScope = role.RoleScope, 
            CanSeeSensitiveData = role.CanSeeSensitiveData,
            Claims = new List<string>(),
            RecordStatus = role.RecordStatus
        };

        var roleClaims = await _claimService.GetRoleClaimsAsync(role);

        foreach (var roleClaim in roleClaims)
        {
            resultRoleDetail.Claims.Add(roleClaim.Value);
        }

        return resultRoleDetail;
    }
}