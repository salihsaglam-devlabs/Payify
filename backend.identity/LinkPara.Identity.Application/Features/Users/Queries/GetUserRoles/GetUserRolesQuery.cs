using AutoMapper;
using LinkPara.Identity.Application.Features.Roles.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserRole;

public class GetUserRolesQuery : IRequest<List<RoleDto>>
{
    public Guid UserId { get; set; }
}

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<RoleDto>>
{
    private readonly IMapper _mapper;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public GetUserRolesQueryHandler(IMapper mapper,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        _mapper = mapper;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<List<RoleDto>> Handle(GetUserRolesQuery command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles is null || userRoles.Count == 0)
        {
            throw new NotFoundException(nameof(Role));
        }

        var roles = await _roleManager.Roles
            .Where(r => userRoles.Contains(r.Name))
            .ToListAsync();

        return _mapper.Map<List<Role>, List<RoleDto>>(roles);
    }
}