using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllMerchantPhysicalDevice;

public class GetAllMerchantPhysicalDeviceQuery : SearchQueryParams, IRequest<PaginatedList<MerchantPhysicalDeviceDto>>
{
    public Guid? MerchantId { get; set; }
    public DeviceType? DeviceType { get; set; }
    public PhysicalPosVendor? PhysicalPosVendor { get; set; }
    public DeviceModel? DeviceModel { get; set; }
    public string SerialNo { get; set; }
    public DeviceStatus? DeviceStatus { get; set; }
    public ContactlessSeparator? ContactlessSeparator { get; set; }
    public string FiscalNo { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetAllMerchantPhysicalDeviceQueryHandler : IRequestHandler<GetAllMerchantPhysicalDeviceQuery, PaginatedList<MerchantPhysicalDeviceDto>>
{
    private readonly IMerchantPhysicalDeviceService _merchantPhysicalDeviceService;

    public GetAllMerchantPhysicalDeviceQueryHandler(IMerchantPhysicalDeviceService merchantPhysicalDeviceService)
    {
        _merchantPhysicalDeviceService = merchantPhysicalDeviceService;
    }
    public async Task<PaginatedList<MerchantPhysicalDeviceDto>> Handle(GetAllMerchantPhysicalDeviceQuery request, CancellationToken cancellationToken)
    {
        return await _merchantPhysicalDeviceService.GetAllAsync(request);
    }
}
