using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.DeviceInventoryHistories;
using LinkPara.PF.Application.Features.DeviceInventoryHistories.Queries.GetAllDeviceInventoryHistories;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class DeviceInventoryHistoryService : IDeviceInventoryHistoryService
{
    private readonly IGenericRepository<DeviceInventoryHistory> _repository;
    private readonly IMapper _mapper;
    public DeviceInventoryHistoryService(IGenericRepository<DeviceInventoryHistory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }


    public async Task CreateSnapshotHistory(Guid deviceInventoryId, DeviceHistoryType historyType, string oldData, string newData, string detail, string createdBy)
    {
        var history = new DeviceInventoryHistory
        {
            DeviceInventoryId = deviceInventoryId,
            DeviceHistoryType = historyType,
            OldData = oldData,
            NewData = newData,
            Detail = detail,
            CreatedNameBy = createdBy
        };

        await _repository.AddAsync(history);
    }

    public async Task<PaginatedList<DeviceInventoryHistoryDto>> GetAllAsync(GetAllDeviceInventoryHistoryQuery request)
    {
        var deviceInventoryHistoryList = _repository.GetAll().Include(b => b.DeviceInventory).AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.DeviceInventory.SerialNo.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.DeviceInventoryHistoryId is not null)
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.Id == request.DeviceInventoryHistoryId);
        }

        if (request.DeviceInventoryId is not null)
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.DeviceInventoryId == request.DeviceInventoryId);
        }

        if (request.DeviceModel is not null)
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.DeviceInventory.DeviceModel == request.DeviceModel);
        }

        if (request.DeviceType is not null)
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.DeviceInventory.DeviceType == request.DeviceType);
        }

        if (request.PhysicalPosVendor is not null)
        {
            deviceInventoryHistoryList = deviceInventoryHistoryList.Where(b => b.DeviceInventory.PhysicalPosVendor == request.PhysicalPosVendor);
        }

        return await deviceInventoryHistoryList.OrderByDescending(b => b.CreateDate)
           .PaginatedListWithMappingAsync<DeviceInventoryHistory, DeviceInventoryHistoryDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
