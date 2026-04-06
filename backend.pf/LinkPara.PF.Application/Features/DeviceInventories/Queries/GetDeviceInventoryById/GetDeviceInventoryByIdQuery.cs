using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventories.Queries.GetDeviceInventoryById;

public class GetDeviceInventoryByIdQuery : IRequest<DeviceInventoryDto>
{
    public Guid Id { get; set; }
}

public class GetDeviceInventoryByIdQueryHandler : IRequestHandler<GetDeviceInventoryByIdQuery, DeviceInventoryDto>
{
    private readonly IDeviceInventoryService _deviceInventoryService;

    public GetDeviceInventoryByIdQueryHandler(IDeviceInventoryService deviceInventoryService)
    {
        _deviceInventoryService = deviceInventoryService;
    }
    public async Task<DeviceInventoryDto> Handle(GetDeviceInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _deviceInventoryService.GetByIdAsync(request.Id);
    }
}