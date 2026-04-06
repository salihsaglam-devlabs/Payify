using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.DeviceInventories.Queries.GetAllDeviceInventories;

public class GetAllDeviceInventoryQuery : SearchQueryParams, IRequest<PaginatedList<DeviceInventoryDto>>
{
    public string SerialNo { get; set; }
    public Guid? MerchantId { get; set; }
    public ContactlessSeparator? ContactlessSeparator { get; set; }
    public PhysicalPosVendor? PhysicalPosVendor { get; set; }
    public DeviceModel? DeviceModel { get; set; }
    public DeviceStatus? DeviceStatus { get; set; }
    public DeviceType? DeviceType { get; set; }
    public InventoryType? InventoryType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetAllDeviceInventoryQueryHandler : IRequestHandler<GetAllDeviceInventoryQuery, PaginatedList<DeviceInventoryDto>>
{
    private readonly IDeviceInventoryService _deviceInventoryService;

    public GetAllDeviceInventoryQueryHandler(IDeviceInventoryService deviceInventoryService)
    {
        _deviceInventoryService = deviceInventoryService;
    }
    public async Task<PaginatedList<DeviceInventoryDto>> Handle(GetAllDeviceInventoryQuery request, CancellationToken cancellationToken)
    {
        return await _deviceInventoryService.GetAllAsync(request);
    }
}
