using LinkPara.Identity.Application.Common.Interfaces;
using MediatR;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.DeleteUserDeviceCommand;
public class DeleteUserDeviceCommand : IRequest
{
    public string DeviceId { get; set; }
}

public class DeleteUserDeviceCommandHandler : IRequestHandler<DeleteUserDeviceCommand>
{
    private readonly IDeviceInfoService _deviceInfo;

    public DeleteUserDeviceCommandHandler(IDeviceInfoService deviceInfo)
    {
        _deviceInfo = deviceInfo;
    }

    public async Task<Unit> Handle(DeleteUserDeviceCommand deleteUserDeviceCommand, CancellationToken cancellationToken)
    {

        await _deviceInfo.DeleteUserDeviceAsync(deleteUserDeviceCommand);

        return Unit.Value;
    }
}