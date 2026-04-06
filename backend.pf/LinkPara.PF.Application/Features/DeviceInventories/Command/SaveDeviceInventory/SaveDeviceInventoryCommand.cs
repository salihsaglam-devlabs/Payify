using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventories.Command.SaveDeviceInventory;

public class SaveDeviceInventoryCommand : IRequest
{
    public List<string> SerialNo { get; set; }
    public ContactlessSeparator ContactlessSeparator { get; set; }
    public PhysicalPosVendor PhysicalPosVendor { get; set; }
    public DeviceModel DeviceModel { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public DeviceType DeviceType { get; set; }
    public InventoryType InventoryType { get; set; }
}

public class SaveDeviceInventoryCommandHandler : IRequestHandler<SaveDeviceInventoryCommand>
{
    private readonly IDeviceInventoryService _deviceInventoryService;

    public SaveDeviceInventoryCommandHandler(IDeviceInventoryService deviceInventoryService)
    {
        _deviceInventoryService = deviceInventoryService;
    }

    public async Task<Unit> Handle(SaveDeviceInventoryCommand request, CancellationToken cancellationToken)
    {
        await _deviceInventoryService.SaveAsync(request);

        return Unit.Value;
    }
}
