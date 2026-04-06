using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalPos;

public class SaveMerchantPhysicalPosCommand : IRequest
{
    public Guid Id { get; set; }
    public List<Guid> PhysicalPosIdList { get; set; }
}

public class SaveMerchantPhysicalPosCommandHandler : IRequestHandler<SaveMerchantPhysicalPosCommand>
{
    private readonly IMerchantPhysicalDeviceService _merchantPhysicalDeviceService;

    public SaveMerchantPhysicalPosCommandHandler(IMerchantPhysicalDeviceService merchantPhysicalDeviceService)
    {
        _merchantPhysicalDeviceService = merchantPhysicalDeviceService;
    }

    public async Task<Unit> Handle(SaveMerchantPhysicalPosCommand request, CancellationToken cancellationToken)
    {
        await _merchantPhysicalDeviceService.SaveMerchantPhysicalPosAsync(request);

        return Unit.Value;
    }
}
