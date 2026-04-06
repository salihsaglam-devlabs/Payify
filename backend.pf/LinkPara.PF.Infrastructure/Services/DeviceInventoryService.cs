using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.DeviceInventories;
using LinkPara.PF.Application.Features.DeviceInventories.Command.DeleteDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.SaveDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Command.UpdateDeviceInventory;
using LinkPara.PF.Application.Features.DeviceInventories.Queries.GetAllDeviceInventories;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services;

public class DeviceInventoryService : IDeviceInventoryService
{
    private readonly ILogger<DeviceInventoryService> _logger;
    private readonly IGenericRepository<DeviceInventory> _repository;
    private readonly IGenericRepository<MerchantPhysicalDevice> _physicalDeviceRepository;
    private readonly IGenericRepository<DeviceInventoryHistory> _deviceInventoryHistoryRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;
    private readonly IUserService _userService;
    public DeviceInventoryService(ILogger<DeviceInventoryService> logger, IGenericRepository<DeviceInventory> repository, IAuditLogService auditLogService, IContextProvider contextProvider, PfDbContext dbContext, IGenericRepository<DeviceInventoryHistory> deviceInventoryHistoryRepository, IUserService userService, IGenericRepository<MerchantPhysicalDevice> physicalDeviceRepository)
    {
        _logger = logger;
        _repository = repository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _dbContext = dbContext;
        _deviceInventoryHistoryRepository = deviceInventoryHistoryRepository;
        _userService = userService;
        _physicalDeviceRepository = physicalDeviceRepository;
    }
    public async Task DeleteAsync(DeleteDeviceInventoryCommand command)
    {
        var deviceInventory = await _repository.GetByIdAsync(command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (deviceInventory is null)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "DeleteDeviceInventory",
                    SourceApplication = "PF",
                    Resource = "DeviceInventory",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"ErrorMessage" , "NotFoundException"}
                    }
                });

            throw new NotFoundException(nameof(PhysicalPos), command.Id);
        }

        try
        {
            if (deviceInventory.DeviceStatus == DeviceStatus.Available)
            {
                var hasActivePos = await _physicalDeviceRepository.GetAll()
                   .Where(d => d.DeviceInventoryId == deviceInventory.Id && d.RecordStatus == RecordStatus.Active)
                   .SelectMany(d => d.MerchantPhysicalPosList)
                   .AnyAsync(p => p.RecordStatus == RecordStatus.Active);

                if (hasActivePos)
                {
                    throw new AlreadyInUseException(nameof(MerchantPhysicalPos));
                }
            }

            deviceInventory.RecordStatus = RecordStatus.Passive;
            deviceInventory.DeviceStatus = DeviceStatus.Passive;

            await _repository.UpdateAsync(deviceInventory);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteDeviceInventory",
                    SourceApplication = "PF",
                    Resource = "DeviceInventory",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()}
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"DeviceInventoryDeleteError : {exception}");
        }
    }

    public async Task<PaginatedList<DeviceInventoryDto>> GetAllAsync(GetAllDeviceInventoryQuery request)
    {
        var deviceInventoryList = _repository.GetAll().Include(b => b.MerchantPhysicalDevices.Where(c => c.RecordStatus == RecordStatus.Active)).ThenInclude(b => b.Merchant).AsQueryable();

        if (!string.IsNullOrEmpty(request.SerialNo))
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.SerialNo.ToLower().Contains(request.SerialNo.ToLower()));
        }

        if (request.MerchantId is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.MerchantPhysicalDevices != null &&
                    b.MerchantPhysicalDevices.Any(m => m.MerchantId == request.MerchantId));
        }

        if (request.ContactlessSeparator is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.ContactlessSeparator == request.ContactlessSeparator);
        }

        if (request.DeviceModel is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.DeviceModel == request.DeviceModel);
        }

        if (request.InventoryType is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.InventoryType == request.InventoryType);
        }

        if (request.DeviceType is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.DeviceType == request.DeviceType);
        }

        if (request.DeviceStatus is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.DeviceStatus == request.DeviceStatus);
        }

        if (request.PhysicalPosVendor is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.PhysicalPosVendor == request.PhysicalPosVendor);
        }

        if (request.RecordStatus is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.CreateDateStart is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            deviceInventoryList = deviceInventoryList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        var query = deviceInventoryList
          .Select(b => new DeviceInventoryDto
          {
              Id = b.Id,
              SerialNo = b.SerialNo,
              ContactlessSeparator = b.ContactlessSeparator,
              PhysicalPosVendor = b.PhysicalPosVendor,
              DeviceModel = b.DeviceModel,
              DeviceStatus = b.DeviceStatus,
              DeviceType = b.DeviceType,
              InventoryType = b.InventoryType,
              CreateDate = b.CreateDate,
              RecordStatus = b.RecordStatus,

              Merchant = b.MerchantPhysicalDevices
                  .Where(m => m.RecordStatus == RecordStatus.Active)
                  .Select(m => new TransactionMerchantResponse
                  {
                      Name = m.Merchant.Name,
                      Number = m.Merchant.Number
                  })
                  .FirstOrDefault()
          });

        return await query.OrderBy(b => b.SerialNo)
             .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<DeviceInventoryDto> GetByIdAsync(Guid id)
    {
        var deviceInventory = await _repository.GetAll()
            .Include(b => b.MerchantPhysicalDevices.Where(c => c.RecordStatus == RecordStatus.Active)).ThenInclude(b => b.Merchant)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (deviceInventory is null)
        {
            throw new NotFoundException(nameof(DeviceInventory), id);
        }

        var dto = new DeviceInventoryDto
        {
            Id = deviceInventory.Id,
            SerialNo = deviceInventory.SerialNo,
            ContactlessSeparator = deviceInventory.ContactlessSeparator,
            PhysicalPosVendor = deviceInventory.PhysicalPosVendor,
            DeviceModel = deviceInventory.DeviceModel,
            DeviceStatus = deviceInventory.DeviceStatus,
            DeviceType = deviceInventory.DeviceType,
            InventoryType = deviceInventory.InventoryType,
            CreateDate = deviceInventory.CreateDate,
            RecordStatus = deviceInventory.RecordStatus,

            Merchant = deviceInventory.MerchantPhysicalDevices
        .Where(m => m.RecordStatus == RecordStatus.Active)
        .Select(m => new TransactionMerchantResponse
        {
            Name = m.Merchant.Name,
            Number = m.Merchant.Number
        })
        .FirstOrDefault()
        };

        return dto;
    }

    public async Task SaveAsync(SaveDeviceInventoryCommand command)
    {
        var activeDeviceInventory = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.DeviceModel == command.DeviceModel
            && b.PhysicalPosVendor == command.PhysicalPosVendor
            && b.DeviceType == command.DeviceType
            && b.SerialNo.Equals(command.SerialNo)
            && b.RecordStatus == RecordStatus.Active);

        if (activeDeviceInventory is not null)
        {
            throw new DuplicateRecordException(nameof(DeviceInventory), command.SerialNo);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var deviceInventories = command.SerialNo
                  .Where(serialNo => !string.IsNullOrWhiteSpace(serialNo))
                  .Select(serialNo => new DeviceInventory
                  {
                      SerialNo = serialNo,
                      ContactlessSeparator = command.ContactlessSeparator,
                      DeviceModel = command.DeviceModel,
                      DeviceType = command.DeviceType,
                      InventoryType = command.InventoryType,
                      PhysicalPosVendor = command.PhysicalPosVendor,
                      DeviceStatus = command.DeviceStatus
                  })
                  .ToList();

                await _repository.AddRangeAsync(deviceInventories);

                var user = await _userService.GetUserAsync(parseUserId);

                var deviceHistories = deviceInventories.Select(device => new DeviceInventoryHistory
                {
                    DeviceInventoryId = device.Id,
                    DeviceHistoryType = DeviceHistoryType.AddDeviceToInventory,
                    OldData = null,
                    NewData = device.SerialNo,
                    Detail = "Device added to inventory",
                    CreatedNameBy = user is not null ? (user.FirstName + " " + user.LastName) : string.Empty,
                }).ToList();

                await _deviceInventoryHistoryRepository.AddRangeAsync(deviceHistories);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SaveDeviceInventory",
                        SourceApplication = "PF",
                        Resource = "DeviceInventory",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        {"SerialNo", command.SerialNo.ToString()},
                        }
                    });
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"DeviceInventorySaveError : {exception}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateDeviceInventoryCommand command)
    {
        var deviceInventory = await _repository.GetAll()
              .FirstOrDefaultAsync(b => b.Id == command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (deviceInventory is null)
        {
            throw new NotFoundException(nameof(PhysicalPos), command.Id);
        }

        var activeDeviceInventory = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.DeviceModel == command.DeviceModel
            && b.PhysicalPosVendor == command.PhysicalPosVendor
            && b.DeviceType == command.DeviceType
            && b.SerialNo.Equals(command.SerialNo)
            && b.RecordStatus == RecordStatus.Active
            && b.Id != deviceInventory.Id);

        if (activeDeviceInventory is not null)
        {
            throw new DuplicateRecordException(nameof(DeviceInventory), command.SerialNo);
        }

        var oldDeviceStatus = deviceInventory.DeviceStatus;

        try
        {

            deviceInventory.SerialNo = command.SerialNo;
            deviceInventory.ContactlessSeparator = command.ContactlessSeparator;
            deviceInventory.DeviceModel = command.DeviceModel;
            deviceInventory.DeviceType = command.DeviceType;
            deviceInventory.InventoryType = command.InventoryType;
            deviceInventory.PhysicalPosVendor = command.PhysicalPosVendor;
            deviceInventory.DeviceStatus = command.DeviceStatus;

            await _repository.UpdateAsync(deviceInventory);

            if (oldDeviceStatus != command.DeviceStatus)
            {
                var user = await _userService.GetUserAsync(parseUserId);

                var deviceHistory = new DeviceInventoryHistory
                {
                    DeviceInventoryId = deviceInventory.Id,
                    DeviceHistoryType = DeviceHistoryType.StatusChange,
                    OldData = oldDeviceStatus.ToString(),
                    NewData = command.DeviceStatus.ToString(),
                    Detail = $"Device status updated from {oldDeviceStatus.ToString()} to {command.DeviceStatus.ToString()}",
                    CreatedNameBy = user is not null ? (user.FirstName + " " + user.LastName) : string.Empty,
                };

                await _deviceInventoryHistoryRepository.AddAsync(deviceHistory);
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateDeviceInventory",
                    SourceApplication = "PF",
                    Resource = "DeviceInventory",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString()},
                        {"SerialNo", command.SerialNo.ToString()},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"DeviceInventoryUpdateError : {exception}");
            throw;
        }
    }
}
