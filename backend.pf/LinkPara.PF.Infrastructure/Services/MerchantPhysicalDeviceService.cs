using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.IKS;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.DeleteMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalPos;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.UpdateMerchantPhysicalDevice;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllMerchantPhysicalDevice;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantPhysicalDeviceService : IMerchantPhysicalDeviceService
{
    private readonly ILogger<MerchantPhysicalDeviceService> _logger;
    private readonly IGenericRepository<MerchantPhysicalDevice> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;
    private readonly IGenericRepository<DeviceInventory> _deviceInventoryRepository;
    private readonly IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> _physicalPosRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IBus _bus;
    private readonly IIksPfService _iksPfService;
    private readonly IUserService _userService;
    private readonly IGenericRepository<DeviceInventoryHistory> _deviceInventoryHistoryRepository;
    private readonly IDeviceInventoryHistoryService _deviceInventoryHistoryService;
    private readonly IVaultClient _vaultClient;
    public MerchantPhysicalDeviceService(ILogger<MerchantPhysicalDeviceService> logger, IGenericRepository<MerchantPhysicalDevice> repository, IMapper mapper, IAuditLogService auditLogService, IContextProvider contextProvider, PfDbContext dbContext, IApiKeyGenerator apiKeyGenerator, IGenericRepository<DeviceInventory> deviceInventoryRepository, IBus bus, IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository, IIksPfService iksPfService, IGenericRepository<DeviceInventoryHistory> deviceInventoryHistoryRepository, IUserService userService, IDeviceInventoryHistoryService deviceInventoryHistoryService, IGenericRepository<Merchant> merchantRepository, IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> physicalPosRepository, IVaultClient vaultClient)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _dbContext = dbContext;
        _apiKeyGenerator = apiKeyGenerator;
        _deviceInventoryRepository = deviceInventoryRepository;
        _bus = bus;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
        _iksPfService = iksPfService;
        _deviceInventoryHistoryRepository = deviceInventoryHistoryRepository;
        _userService = userService;
        _deviceInventoryHistoryService = deviceInventoryHistoryService;
        _merchantRepository = merchantRepository;
        _physicalPosRepository = physicalPosRepository;
        _vaultClient = vaultClient;
    }

    public async Task DeleteMerchantPhysicalPosAsync(DeleteMerchantPhysicalPosCommand command)
    {
        var merchantPhysicalPos = await _merchantPhysicalPosRepository.GetAll().Include(c=>c.PhysicalPos).Include(b => b.MerchantPhysicalDevice).ThenInclude(c => c.Merchant)
            .FirstOrDefaultAsync(b => b.Id == command.MerchantPhysicalPosId);

        if (merchantPhysicalPos is null)
        {
            throw new NotFoundException(nameof(MerchantPhysicalPos), command.MerchantPhysicalPosId);
        }

        try
        {
            var isSuccess = true;

            merchantPhysicalPos.RecordStatus = RecordStatus.Passive;
            merchantPhysicalPos.TerminalStatus = TerminalStatus.Passive;

            var isIksEnabled =
           await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "IksEnabled");

            if (isIksEnabled)
            {
                isSuccess = await _iksPfService.IKSUpdatePhysicalTerminalAsync(merchantPhysicalPos.MerchantPhysicalDevice.Merchant, merchantPhysicalPos);
            }
            
            if (isSuccess)
            {
                await _merchantPhysicalPosRepository.UpdateAsync(merchantPhysicalPos);

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                var user = await _userService.GetUserAsync(parseUserId);
                var createdBy = user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty;

                await _deviceInventoryHistoryService.CreateSnapshotHistory(merchantPhysicalPos.MerchantPhysicalDevice.DeviceInventoryId, DeviceHistoryType.RemoveBank, merchantPhysicalPos.PhysicalPos.Name, null, $"Physical Pos removed to merchant {merchantPhysicalPos.MerchantPhysicalDevice.MerchantId}", createdBy);
            }
            else
            {
                throw new InvalidUpdateIksException();
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"DeleteMerchantPhysicalPos : {exception}");
        }
    }

    public async Task<PaginatedList<MerchantPhysicalDeviceDto>> GetAllAsync(GetAllMerchantPhysicalDeviceQuery request)
    {
        var merchantPhysicalDeviceList = _repository.GetAll().Include(b => b.Merchant).Include(b => b.DeviceInventory).AsQueryable();

        if (!string.IsNullOrEmpty(request.FiscalNo))
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.FiscalNo.ToLower().Contains(request.Q.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.SerialNo))
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.SerialNo.ToLower().Contains(request.SerialNo.ToLower()));
        }

        if (request.MerchantId is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.ContactlessSeparator is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.ContactlessSeparator == request.ContactlessSeparator);
        }

        if (request.DeviceModel is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.DeviceModel == request.DeviceModel);
        }

        if (request.DeviceType is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.DeviceType == request.DeviceType);
        }

        if (request.DeviceStatus is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.DeviceStatus == request.DeviceStatus);
        }

        if (request.PhysicalPosVendor is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.DeviceInventory.PhysicalPosVendor == request.PhysicalPosVendor);
        }

        if (request.RecordStatus is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.CreateDateStart is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            merchantPhysicalDeviceList = merchantPhysicalDeviceList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        merchantPhysicalDeviceList = merchantPhysicalDeviceList
            .Include(b => b.MerchantPhysicalPosList.Where(c => c.RecordStatus == RecordStatus.Active)).ThenInclude(b => b.PhysicalPos).ThenInclude(b => b.AcquireBank).ThenInclude(b => b.Bank);

        return await merchantPhysicalDeviceList
            .PaginatedListWithMappingAsync<MerchantPhysicalDevice, MerchantPhysicalDeviceDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveMerchantPhysicalDeviceCommand command)
    {
        var inventories = await _deviceInventoryRepository.GetAll()
             .Where(x => command.SaveMerchantPhysicalDeviceList.Select(c => c.DeviceInventoryId).Contains(x.Id))
             .ToListAsync();

        if (inventories.Any(x => x.DeviceStatus != DeviceStatus.Available && x.DeviceStatus != DeviceStatus.Occupied))
            throw new InvalidDataException("Kullanılamayan cihaz seçildi.");

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                foreach (var merchantPhysicalDeviceItem in command.SaveMerchantPhysicalDeviceList)
                {

                    var activeMerchantPhysicalDevice = await _repository.GetAll()
                         .FirstOrDefaultAsync(b => b.MerchantId == command.MerchantId
                         && b.DeviceInventoryId == merchantPhysicalDeviceItem.DeviceInventoryId
                         && b.RecordStatus == RecordStatus.Active);

                    if (activeMerchantPhysicalDevice is not null)
                    {
                        throw new DuplicateRecordException(nameof(MerchantPhysicalDevice), merchantPhysicalDeviceItem.DeviceInventoryId.ToString());
                    }

                    var merchantApiKeyDto = await _apiKeyGenerator.Generate(command.MerchantId);

                    var merchantDevice = new MerchantPhysicalDevice
                    {
                        MerchantId = command.MerchantId,
                        DeviceInventoryId = merchantPhysicalDeviceItem.DeviceInventoryId,
                        IsPinPad = merchantPhysicalDeviceItem.IsPinPad,
                        ConnectionType = merchantPhysicalDeviceItem.ConnectionType,
                        AssignmentType = merchantPhysicalDeviceItem.AssignmentType,
                        OwnerPspNo = merchantPhysicalDeviceItem.OwnerPspNo,
                        FiscalNo = merchantPhysicalDeviceItem.FiscalNo,
                        OwnerTerminalId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),

                        MerchantPhysicalPosList = command.PhysicalPosIdList
                            .Select(pyhsicalPosId => new MerchantPhysicalPos
                            {
                                PhysicalPosId = pyhsicalPosId,
                                TerminalStatus = TerminalStatus.PendingRequest,
                                DeviceTerminalStatus = DeviceTerminalStatus.Unknown,
                                DeviceTerminalLastActivity = DateTime.MinValue
                            }).ToList(),

                        DeviceApiKeys = new List<MerchantDeviceApiKey>
                            {
                                new MerchantDeviceApiKey
                                {
                                    PublicKey = merchantApiKeyDto.PublicKey,
                                    PrivateKeyEncrypted = merchantApiKeyDto.PrivateKeyEncrypted
                                }
                            }
                    };

                    await _repository.AddAsync(merchantDevice);

                    var deviceInventory = await _deviceInventoryRepository.GetByIdAsync(merchantPhysicalDeviceItem.DeviceInventoryId);

                    var oldDeviceStatus = deviceInventory.DeviceStatus; 
                    deviceInventory.DeviceStatus = DeviceStatus.Occupied;

                    await _deviceInventoryRepository.UpdateAsync(deviceInventory);

                    var user = await _userService.GetUserAsync(parseUserId);
                    var createdBy = user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty;
                    var merchantName = await _merchantRepository.GetAll().AsNoTracking().Where(x => x.Id == command.MerchantId).Select(x => x.Name).FirstOrDefaultAsync();

                    await _deviceInventoryHistoryService.CreateSnapshotHistory(deviceInventory.Id, DeviceHistoryType.StatusChange, oldDeviceStatus.ToString(), deviceInventory.DeviceStatus.ToString(), $"Device status updated from {oldDeviceStatus.ToString()} to {deviceInventory.DeviceStatus.ToString()}", createdBy);

                    await _deviceInventoryHistoryService.CreateSnapshotHistory(deviceInventory.Id, DeviceHistoryType.AddToMerchant, null, merchantName, $"Device assigned to merchant {command.MerchantId}", createdBy);

                }                        

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SaveMerchantPhysicalDevice",
                        SourceApplication = "PF",
                        Resource = "MerchantPhysicalDevice",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        {"MerchantId", command.MerchantId.ToString()},
                        }
                    });

                scope.Complete();
            });

            var isIksEnabled = await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "IksEnabled");

            if (isIksEnabled)
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.CreateIksTerminal"));
                await endpoint.Send(new CreateIksTerminal
                {
                    MerchantId = command.MerchantId,
                    PosType = PosType.Physical
                }, tokenSource.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantPhysicalDeviceSaveError : {exception}");
            throw;
        }
    }

    public async Task SaveMerchantPhysicalPosAsync(SaveMerchantPhysicalPosCommand command)
    {
        var merchantPhysicalDevice = await _repository.GetAll().Include(b => b.MerchantPhysicalPosList).FirstOrDefaultAsync(b => b.Id == command.Id);

        if (merchantPhysicalDevice is null || !command.PhysicalPosIdList.Any())
        {
            throw new NotFoundException(nameof(MerchantPhysicalDevice), command.Id);
        }

        try
        {
            var existingPosIds = merchantPhysicalDevice.MerchantPhysicalPosList
             .Select(x => x.PhysicalPosId)
             .ToHashSet();

            var newEntities = command.PhysicalPosIdList
             .Except(existingPosIds)
             .Select(id => new MerchantPhysicalPos
             {
                 PhysicalPosId = id,
                 MerchantPhysicalDeviceId = merchantPhysicalDevice.Id,
                 RecordStatus = RecordStatus.Active,
                 TerminalStatus = TerminalStatus.PendingRequest,
                 DeviceTerminalStatus = DeviceTerminalStatus.Unknown,
                 DeviceTerminalLastActivity = DateTime.MinValue
             }).ToList();

            await _merchantPhysicalPosRepository.AddRangeAsync(newEntities);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _userService.GetUserAsync(parseUserId);
            var createdBy = user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty;

            var newPosIds = newEntities.Select(x => x.PhysicalPosId).ToList();

            var posInfos = await _physicalPosRepository.GetAll().AsNoTracking()
                .Where(x => newPosIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .ToListAsync();

            var posDictionary = posInfos.ToDictionary(x => x.Id, x => x.Name);

            var deviceHistories = newEntities.Select(pos => new DeviceInventoryHistory
            {
                DeviceInventoryId = merchantPhysicalDevice.DeviceInventoryId,
                DeviceHistoryType = DeviceHistoryType.AddBank,
                OldData = null,
                NewData = posDictionary.ContainsKey(pos.PhysicalPosId)
                ? posDictionary[pos.PhysicalPosId]
                : null,
                 Detail = $"PosId: {pos.PhysicalPosId}",
                 CreatedNameBy = createdBy
            }).ToList();

            await _deviceInventoryHistoryRepository.AddRangeAsync(deviceHistories);

            var isIksEnabled =
         await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "IksEnabled");

            if (isIksEnabled)
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.CreateIksTerminal"));
                await endpoint.Send(new CreateIksTerminal
                {
                    MerchantId = merchantPhysicalDevice.MerchantId,
                    PosType = PosType.Physical
                }, tokenSource.Token);
            }          

        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveMerchantPhysicalPosAsync : {exception}");
        }
    }

    public async Task UpdateAsync(UpdateMerchantPhysicalDeviceCommand command)
    {
        var merchantPhysicalDevice = await _repository.GetAll().Include(b => b.MerchantPhysicalPosList).Include(b => b.DeviceInventory).FirstOrDefaultAsync(b => b.Id == command.Id);

        if (merchantPhysicalDevice is null)
        {
            throw new NotFoundException(nameof(MerchantPhysicalDevice), command.Id);
        }

        if (merchantPhysicalDevice.DeviceInventory is null)
        {
            throw new NotFoundException(nameof(DeviceInventory), merchantPhysicalDevice.DeviceInventoryId);
        }

        if (merchantPhysicalDevice.MerchantPhysicalPosList.Where(b => b.RecordStatus == RecordStatus.Active).Any())
        {
            throw new AlreadyInUseException(nameof(MerchantPhysicalPos));
        }

        try
        {
            if (command.DeviceStatus != DeviceStatus.Available && command.DeviceStatus != DeviceStatus.Occupied)
            {
                var oldDeviceStatus = merchantPhysicalDevice.DeviceInventory.DeviceStatus;

                merchantPhysicalDevice.RecordStatus = RecordStatus.Passive;

                merchantPhysicalDevice.DeviceInventory.DeviceStatus = command.DeviceStatus;
                merchantPhysicalDevice.DeviceInventory.RecordStatus = RecordStatus.Passive;

                await _repository.UpdateAsync(merchantPhysicalDevice);
                await _deviceInventoryRepository.UpdateAsync(merchantPhysicalDevice.DeviceInventory);

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                var user = await _userService.GetUserAsync(parseUserId);
                var createdBy = user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty;
                var merchantName = await _merchantRepository.GetAll().AsNoTracking().Where(x => x.Id == merchantPhysicalDevice.MerchantId).Select(x => x.Name).FirstOrDefaultAsync();

                await _deviceInventoryHistoryService.CreateSnapshotHistory(merchantPhysicalDevice.DeviceInventoryId, DeviceHistoryType.StatusChange, oldDeviceStatus.ToString(), command.DeviceStatus.ToString(), $"Device status updated from {oldDeviceStatus.ToString()} to {command.DeviceStatus.ToString()}", createdBy);

                await _deviceInventoryHistoryService.CreateSnapshotHistory(merchantPhysicalDevice.DeviceInventoryId, DeviceHistoryType.RemoveFromMerchant, merchantName, null, $"Device removed to merchant {merchantPhysicalDevice.MerchantId}", createdBy);

            }
            else
            {
                throw new InvalidParameterException(command.DeviceStatus.ToString());
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateMerchantPhysicalDevice : {exception}");
        }
    }
}
