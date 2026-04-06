using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.DeleteMerchantPhysicalPos;

public class DeleteMerchantPhysicalPosCommand : IRequest
{
    public Guid MerchantPhysicalPosId { get; set; }
}

public class DeleteMerchantPhysicalPosCommandHandler : IRequestHandler<DeleteMerchantPhysicalPosCommand>
{
    private readonly IMerchantPhysicalDeviceService _merchantPhysicalDeviceService;

    public DeleteMerchantPhysicalPosCommandHandler(IMerchantPhysicalDeviceService merchantPhysicalDeviceService)
    {
        _merchantPhysicalDeviceService = merchantPhysicalDeviceService;
    }

    public async Task<Unit> Handle(DeleteMerchantPhysicalPosCommand request, CancellationToken cancellationToken)
    {
        await _merchantPhysicalDeviceService.DeleteMerchantPhysicalPosAsync(request);

        return Unit.Value;
    }
}