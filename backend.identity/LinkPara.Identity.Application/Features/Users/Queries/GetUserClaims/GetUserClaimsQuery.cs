using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AuthorizationModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserClaims;

public class GetUserClaimsQuery : IRequest<List<ClaimDto>>
{
    public Guid UserId { get; set; }
}

public class GetUserClaimsQueryHandler : IRequestHandler<GetUserClaimsQuery, List<ClaimDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IClaimService _claimService;

    public GetUserClaimsQueryHandler(UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IClaimService claimService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _claimService = claimService;
    }

    public async Task<List<ClaimDto>> Handle(GetUserClaimsQuery command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }

        var result = new List<ClaimDto>();

        foreach (var roleName in await _userManager.GetRolesAsync(user))
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            result.Add(new ClaimDto { Type = ClaimKey.RoleScope, Values = roleClaims.Select(q => q.Value)?.ToArray() });
        }

        var userClaims = await _claimService.GetUserClaimsAsync(user);
        if (userClaims is not null)
        {
            result.Add(new ClaimDto { Type = ClaimKey.UserScope, Values = userClaims.Where(q => !result.Any(x => x.Values.Contains(q.Value))).Select(q => q.Value)?.ToArray() });
        }

        return result;
    }
}