using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.DeviceInfo;
using MediatR;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.GetUserDeviceInfoCommand;
public class GetUserDeviceInfoCommand : IRequest<List<UserDeviceInfoDto>>
{
    public List<Guid> UserIdList{ get; set; }

}

public class GetUserDeviceInfoCommandHandler : IRequestHandler<GetUserDeviceInfoCommand,List<UserDeviceInfoDto>>
{
    private readonly IDeviceInfoService _deviceInfo;

    public GetUserDeviceInfoCommandHandler(IDeviceInfoService deviceInfo)
    {
        _deviceInfo = deviceInfo;
    }

    public async Task<List<UserDeviceInfoDto>> Handle(GetUserDeviceInfoCommand command, CancellationToken cancellationToken)
    {

        var res = await _deviceInfo.GetUsersDeviceInfoAsync(command.UserIdList);

        return res;
    }
}