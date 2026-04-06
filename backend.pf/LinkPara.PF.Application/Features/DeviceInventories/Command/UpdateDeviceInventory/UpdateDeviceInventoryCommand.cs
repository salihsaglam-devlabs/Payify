using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventories.Command.UpdateDeviceInventory;

public class UpdateDeviceInventoryCommand : IRequest
{
    public Guid Id { get; set; }
    public string SerialNo { get; set; }
    public ContactlessSeparator ContactlessSeparator { get; set; }
    public PhysicalPosVendor PhysicalPosVendor { get; set; }
    public DeviceModel DeviceModel { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public DeviceType DeviceType { get; set; }
    public InventoryType InventoryType { get; set; }
}

public class UpdateDeviceInventoryCommandHandler : IRequestHandler<UpdateDeviceInventoryCommand>
{
    private readonly IDeviceInventoryService _deviceInventoryService;

    public UpdateDeviceInventoryCommandHandler(IDeviceInventoryService deviceInventoryService)
    {
        _deviceInventoryService = deviceInventoryService;
    }

    public async Task<Unit> Handle(UpdateDeviceInventoryCommand request, CancellationToken cancellationToken)
    {
        await _deviceInventoryService.UpdateAsync(request);

        return Unit.Value;
    }
}
