using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Billing.Infrastructure.Consumers.CronJobs;

public class SectorConsumer : IConsumer<SyncBillingSectors>
{
    private readonly ILogger<SectorConsumer> _logger;
    private readonly IBillingVendorServiceFactory _billingServiceFactory;
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly IGenericRepository<Sector> _sectorReposistory;
    private readonly IGenericRepository<SynchronizationLog> _synchronizationLogRepository;
    private readonly IVendorMapper _vendorMapper;
    private readonly IApplicationUserService _applicationUserService;

    public SectorConsumer(IBillingVendorServiceFactory billingServiceFactory,
        IGenericRepository<Vendor> vendorRepository,
        IVendorMapper vendorMapper,
        ILogger<SectorConsumer> logger,
        IGenericRepository<Sector> sectorReposistory,
        IGenericRepository<SectorMapping> sectorMappingRepository,
        IGenericRepository<SynchronizationLog> synchronizationLogRepository,
        IApplicationUserService applicationUserService)
    {
        _billingServiceFactory = billingServiceFactory;
        _vendorRepository = vendorRepository;
        _vendorMapper = vendorMapper;
        _logger = logger;
        _sectorReposistory = sectorReposistory;
        _synchronizationLogRepository = synchronizationLogRepository;
        _applicationUserService = applicationUserService;
    }

    public async Task Consume(ConsumeContext<SyncBillingSectors> context)
    {
        try
        {
            var vendorIds = await _vendorRepository.GetAll()
                .Where(v => v.RecordStatus == RecordStatus.Active)
                .Select(v => v.Id)
                .ToListAsync();

            await Task.WhenAll(vendorIds.Select(MapSectorsByVendorAsync));
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorSyncingSectors: {exception}", exception);
        }
    }

    private async Task MapSectorsByVendorAsync(Guid vendorId)
    {
        try
        {
            var vendorBillingService = await _billingServiceFactory.GetBillingServiceAsync(vendorId);
            var vendorSectors = await vendorBillingService.GetSectorListAsync();
            var sectorMappings = await _vendorMapper.GetSectorMappingsByVendorAsync(vendorId);

            foreach (var vendorSector in vendorSectors)
            {
                var sectorMapping = sectorMappings.Find(v => v.VendorSectorId == vendorSector.VendorSectorId);

                if (sectorMapping is null)
                {
                    var sector = await _sectorReposistory.GetAll()
                        .FirstOrDefaultAsync(s => s.Name == vendorSector.VendorSectorId);

                    var synchronizationType = SynchronizationType.NewMapping;

                    if (sector is null)
                    {
                        sector = new Sector
                        {
                            Name = vendorSector.VendorSectorId,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                            RecordStatus = RecordStatus.Active
                        };

                        await _sectorReposistory.AddAsync(sector);

                        synchronizationType = SynchronizationType.New;
                    }

                    sectorMapping = new SectorMapping
                    {
                        SectorId = sector.Id,
                        VendorId = vendorId,
                        VendorSectorId = vendorSector.VendorSectorId,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        RecordStatus = RecordStatus.Active
                    };

                    await _vendorMapper.AddSectorMappingAsync(sectorMapping);
                    await SaveSynchronizationLog(sectorMapping, vendorId, synchronizationType);
                }
                else
                {
                    if (sectorMapping.RecordStatus == RecordStatus.Passive)
                    {
                        sectorMapping.RecordStatus = RecordStatus.Active;

                        await _vendorMapper.UpdateSectorMappingAsync(sectorMapping);
                        await SaveSynchronizationLog(sectorMapping, vendorId, SynchronizationType.Activate);
                    }

                    sectorMappings.Remove(sectorMapping);
                }
            }

            if (sectorMappings.Any())
            {
                foreach (var sectorMapping in sectorMappings)
                {
                    sectorMapping.RecordStatus = RecordStatus.Passive;

                    await _vendorMapper.UpdateSectorMappingAsync(sectorMapping);
                    await SaveSynchronizationLog(sectorMapping, vendorId, SynchronizationType.Disable);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorSyncingSectorsForVendor: {vendorId}, Error: {exception}", vendorId, exception);
        }
    }

    private async Task SaveSynchronizationLog(SectorMapping sectorMapping, Guid vendorId, SynchronizationType synchronizationType)
    {
        try
        {
            var synchronizationLog = new SynchronizationLog
            {
                ItemId = sectorMapping.Sector.Id,
                ItemName = sectorMapping.Sector.Name,
                VendorId = vendorId,
                SynchronizationItem = SynchronizationItem.Sector,
                SynchronizationType = synchronizationType,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };

            await _synchronizationLogRepository.AddAsync(synchronizationLog);
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorLoggingSectorSynchronization: {exception}", exception);
        }
    }
}
