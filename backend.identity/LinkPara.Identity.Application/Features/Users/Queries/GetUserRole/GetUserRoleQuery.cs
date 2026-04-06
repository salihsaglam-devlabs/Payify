using AutoMapper;
using LinkPara.Identity.Application.Features.Roles.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserRole;

public class GetUserRoleQuery : IRequest<RoleDto>
{
    public Guid UserId { get; set; }
}

public class GetUserRoleQueryHandler : IRequestHandler<GetUserRoleQuery, RoleDto>
{
    private readonly IMapper _mapper;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public GetUserRoleQueryHandler(IMapper mapper,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        _mapper = mapper;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<RoleDto> Handle(GetUserRoleQuery command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }
        var userRoles = await _userManager.GetRolesAsync(user);
        
        if (userRoles is null || userRoles.Count == 0 )
        {
            throw new NotFoundException(nameof(Role));
        }

        var userRole = userRoles.FirstOrDefault();

        var role = await _roleManager.FindByNameAsync(userRole);

        return _mapper.Map<Role, RoleDto>(role);
    }
}