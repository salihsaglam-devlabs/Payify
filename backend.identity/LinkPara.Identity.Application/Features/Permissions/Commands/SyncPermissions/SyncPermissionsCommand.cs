using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using MediatR;

namespace LinkPara.Identity.Application.Features.Permissions.Commands.SyncPermissions;

public class SyncPermissionsCommand : IRequest
{
    public List<SyncPermissionDto> Permissions { get; set; }
}

public class SyncPermissionsCommandHandler : IRequestHandler<SyncPermissionsCommand>
{
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;

    public SyncPermissionsCommandHandler(IPermissionService permissionService, IMapper mapper)
    {
        _permissionService = permissionService;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SyncPermissionsCommand command, CancellationToken cancellationToken)
    {
        var permissions = _mapper.Map<List<SyncPermissionDto>, List<Permission>>(command.Permissions);

        await _permissionService.AddRangePermissionAsync(permissions);

        return Unit.Value;
    }
}