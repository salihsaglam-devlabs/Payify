using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.DeviceInfo;
using MediatR;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.CreateDeviceInfo;
public class CreateDeviceInfoCommand : IRequest
{
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
    public string DeviceName { get; set; }
    public string RegistrationToken { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string OperatingSystem { get; set; }
    public string OperatingSystemVersion { get; set; }
    public string ScreenResolution { get; set; }
    public string AppVersion { get; set; }
    public string AppBuildNumber { get; set; }
    public string Camera { get; set; }
}

public class CreateDeviceInfoCommandHandler : IRequestHandler<CreateDeviceInfoCommand>
{
    private readonly IDeviceInfoService _deviceInfo;

    public CreateDeviceInfoCommandHandler(IDeviceInfoService deviceInfo)
    {
        _deviceInfo = deviceInfo;
    }

    public async Task<Unit> Handle(CreateDeviceInfoCommand command, CancellationToken cancellationToken)
    {
        var deviceInfo = new DeviceInfoDto()
        {
            DeviceId = command.DeviceId,
            DeviceType = command.DeviceType,
            DeviceName = command.DeviceName,
            RegistrationToken = command.RegistrationToken,
            Manufacturer = command.Manufacturer,
            Model = command.Model,
            OperatingSystem = command.OperatingSystem,
            OperatingSystemVersion = command.OperatingSystemVersion,
            ScreenResolution = command.ScreenResolution,
            AppVersion = command.AppVersion,
            AppBuildNumber = command.AppBuildNumber,
            Camera = command.Camera,
        };

        await _deviceInfo.AddAsync(deviceInfo);

        return Unit.Value;
    }
}