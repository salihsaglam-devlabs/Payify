using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventories.Command.DeleteDeviceInventory;

public class DeleteDeviceInventoryCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteDeviceInventoryCommandHandler : IRequestHandler<DeleteDeviceInventoryCommand>
{
    private readonly IDeviceInventoryService _deviceInventoryService;

    public DeleteDeviceInventoryCommandHandler(IDeviceInventoryService deviceInventoryService)
    {
        _deviceInventoryService = deviceInventoryService;
    }

    public async Task<Unit> Handle(DeleteDeviceInventoryCommand request, CancellationToken cancellationToken)
    {
        await _deviceInventoryService.DeleteAsync(request);

        return Unit.Value;
    }
}