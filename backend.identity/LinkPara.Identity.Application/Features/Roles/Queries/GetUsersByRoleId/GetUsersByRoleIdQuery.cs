using AutoMapper;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Roles.Queries.GetUsersByRoleId;

public class GetUsersByRoleIdQuery : IRequest<List<UserDto>>
{
    public Guid RoleId { get; set; }
}

public class GetUsersByRoleIdQueryHandler : IRequestHandler<GetUsersByRoleIdQuery, List<UserDto>>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public GetUsersByRoleIdQueryHandler(RoleManager<Role> roleManager, UserManager<User> userManager,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _mapper = mapper;
    }
    public async Task<List<UserDto>> Handle(GetUsersByRoleIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());

        if (role is null)
        {
            throw new NotFoundException(nameof(Role), request.RoleId);
        }

        var userRoles = await _userManager.GetUsersInRoleAsync(role.Name);

        return _mapper.Map<List<UserDto>>(userRoles);
    }
}
