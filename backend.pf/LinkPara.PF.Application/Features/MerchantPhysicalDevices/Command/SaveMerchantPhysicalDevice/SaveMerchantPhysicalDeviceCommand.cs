using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPhysicalDevices;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalDevice;

public class SaveMerchantPhysicalDeviceCommand : IRequest
{
    public Guid MerchantId { get; set; }
    public List<SaveMerchantPhysicalDeviceRequest> SaveMerchantPhysicalDeviceList { get; set; }
    public List<Guid> PhysicalPosIdList { get; set; }
}

public class SaveMerchantPhysicalDeviceCommandHandler : IRequestHandler<SaveMerchantPhysicalDeviceCommand>
{
    private readonly IMerchantPhysicalDeviceService _merchantPhysicalDeviceService;

    public SaveMerchantPhysicalDeviceCommandHandler(IMerchantPhysicalDeviceService merchantPhysicalDeviceService)
    {
        _merchantPhysicalDeviceService = merchantPhysicalDeviceService;
    }

    public async Task<Unit> Handle(SaveMerchantPhysicalDeviceCommand request, CancellationToken cancellationToken)
    {
        await _merchantPhysicalDeviceService.SaveAsync(request);

        return Unit.Value;
    }
}
