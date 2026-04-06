using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventoryHistories.Queries.GetAllDeviceInventoryHistories;

public class GetAllDeviceInventoryHistoryQuery : SearchQueryParams, IRequest<PaginatedList<DeviceInventoryHistoryDto>>
{
    public Guid? DeviceInventoryHistoryId  { get; set; }
    public Guid? DeviceInventoryId { get; set; }
    public DeviceModel? DeviceModel { get; set; }
    public DeviceType? DeviceType { get; set; }
    public PhysicalPosVendor? PhysicalPosVendor { get; set; }
}

public class GetAllDeviceInventoryHistoryQueryHandler : IRequestHandler<GetAllDeviceInventoryHistoryQuery, PaginatedList<DeviceInventoryHistoryDto>>
{
    private readonly IDeviceInventoryHistoryService _deviceInventoryHistoryService;

    public GetAllDeviceInventoryHistoryQueryHandler(IDeviceInventoryHistoryService deviceInventoryHistoryService)
    {
        _deviceInventoryHistoryService = deviceInventoryHistoryService;
    }
    public async Task<PaginatedList<DeviceInventoryHistoryDto>> Handle(GetAllDeviceInventoryHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _deviceInventoryHistoryService.GetAllAsync(request);
    }
}
