using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.UpdateMerchantPhysicalDevice;

public class UpdateMerchantPhysicalDeviceCommand : IRequest
{
    public Guid Id { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
}

public class UpdateMerchantPhysicalDeviceCommandHandler : IRequestHandler<UpdateMerchantPhysicalDeviceCommand>
{
    private readonly IMerchantPhysicalDeviceService _merchantPhysicalDeviceService;

    public UpdateMerchantPhysicalDeviceCommandHandler(IMerchantPhysicalDeviceService merchantPhysicalDeviceService)
    {
        _merchantPhysicalDeviceService = merchantPhysicalDeviceService;
    }

    public async Task<Unit> Handle(UpdateMerchantPhysicalDeviceCommand request, CancellationToken cancellationToken)
    {
        await _merchantPhysicalDeviceService.UpdateAsync(request);

        return Unit.Value;
    }
}
