using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserById;

public class GetAllPermissionsQuery : IRequest<List<PermissionDto>>
{
}

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IPermissionService _permissionService;

    public GetAllPermissionsQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetAllPermissionAsync();

        if (permissions.Count == 0)
        {
            throw new NotFoundException(nameof(Permission));
        }

        return permissions;
    }
}