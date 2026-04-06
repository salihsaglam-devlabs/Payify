using System.Globalization;
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

public class InstitutionConsumer : IConsumer<SyncBillingInstitutions>
{
    private readonly ILogger<InstitutionConsumer> _logger;
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly IGenericRepository<Sector> _sectorRepository;
    private readonly IGenericRepository<Institution> _institutionRepository;
    private readonly IGenericRepository<SynchronizationLog> _synchronizationLogRepository;
    private readonly IBillingVendorServiceFactory _billingServiceFactory;
    private readonly IVendorMapper _vendorMapperService;
    private readonly IApplicationUserService _applicationUserService;

    public InstitutionConsumer(ILogger<InstitutionConsumer> logger,
        IGenericRepository<Vendor> vendorRepository,
        IBillingVendorServiceFactory billingServiceFactory,
        IVendorMapper vendorMapperService,
        IGenericRepository<Sector> sectorRepository,
        IGenericRepository<Institution> institutionRepository,
        IGenericRepository<SynchronizationLog> synchronizationLogRepository,
        IApplicationUserService applicationUserService)
    {
        _logger = logger;
        _vendorRepository = vendorRepository;
        _billingServiceFactory = billingServiceFactory;
        _vendorMapperService = vendorMapperService;
        _sectorRepository = sectorRepository;
        _institutionRepository = institutionRepository;
        _synchronizationLogRepository = synchronizationLogRepository;
        _applicationUserService = applicationUserService;
    }

    public async Task Consume(ConsumeContext<SyncBillingInstitutions> context)
    {
        try
        {
            var vendorIds = await _vendorRepository.GetAll()
                .Where(v => v.RecordStatus == RecordStatus.Active)
                .Select(v => v.Id)
                .ToListAsync();

            foreach (var vendorId in vendorIds)
            {

                await MapInstitutionsByVendorAsync(vendorId);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorSyncingInstitutions: {exception}", exception);
        }
    }

    private async Task MapInstitutionsByVendorAsync(Guid vendorId)
    {
        try
        {
            var vendorBillingService = await _billingServiceFactory.GetBillingServiceAsync(vendorId);
            var vendorInstitutions = await vendorBillingService.GetInstitutionListAsync();
            var institutionMappings = await _vendorMapperService.GetInstitutionMappingsByVendorAsync(vendorId);

            var sectors = await _sectorRepository.GetAll()
                        .Select(s => new { s.Id, s.Name })
                        .ToListAsync();

            var institutions = await _institutionRepository.GetAll().ToListAsync();

            foreach (var vendorInstitution in vendorInstitutions)
            {
                var institutionMapping = institutionMappings.Find(i => i.VendorInstitutionId == vendorInstitution.VendorInstitutionId);

                if (institutionMapping is null)
                {
                    var sector = sectors.Find(s => s.Name == vendorInstitution.Institution.Sector.Name.ToUpper(CultureInfo.InvariantCulture));
                    var institution = institutions.Find(i => i.Name == vendorInstitution.Institution.Name.ToUpper(CultureInfo.InvariantCulture));

                    var synchronizationType = SynchronizationType.NewMapping;
                    Guid institutionId;

                    if (institution is null)
                    {
                        var newInstitution = new Institution
                        {
                            Name = vendorInstitution.Institution.Name.ToUpper(CultureInfo.InvariantCulture),
                            FieldRequirementType = vendorInstitution.Institution.FieldRequirementType,
                            Fields = vendorInstitution.Institution.Fields,
                            OperationMode = vendorInstitution.Institution.OperationMode,
                            SectorId = sector != null ? sector.Id : Guid.Empty,
                            ActiveVendorId = vendorId,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                            RecordStatus = RecordStatus.Active
                        };

                        newInstitution.Fields.ForEach(f => f.CreatedBy = _applicationUserService.ApplicationUserId.ToString());

                        await _institutionRepository.AddAsync(newInstitution);

                        institutionId = newInstitution.Id;
                        institution = newInstitution;
                        synchronizationType = SynchronizationType.New;
                    }
                    else
                    {
                        institutionId = institution.Id;

                        if (institution.RecordStatus == RecordStatus.Passive)
                        {
                            institution.ActiveVendorId = vendorId;
                            institution.RecordStatus = RecordStatus.Active;

                            await _institutionRepository.UpdateAsync(institution);
                        }
                    }

                    institutionMapping = new InstitutionMapping
                    {
                        InstitutionId = institutionId,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        VendorInstitutionId = vendorInstitution.VendorInstitutionId,
                        VendorId = vendorId,
                        RecordStatus = RecordStatus.Active
                    };

                    await _vendorMapperService.AddInstitutionMappingAsync(institutionMapping);
                    await SaveSynchronizationLog(institution, vendorId, synchronizationType);
                }
                else
                {
                    if (institutionMapping.RecordStatus == RecordStatus.Passive)
                    {
                        var institution = institutions.Find(i => i.Id == institutionMapping.InstitutionId);

                        institutionMapping.RecordStatus = RecordStatus.Active;

                        await _vendorMapperService.UpdateInstitutionMappingAsync(institutionMapping);
                        await SaveSynchronizationLog(institution, vendorId, SynchronizationType.Activate);

                        if (institution?.RecordStatus == RecordStatus.Passive)
                        {
                            institution.ActiveVendorId = vendorId;
                            institution.RecordStatus = RecordStatus.Active;

                            await _institutionRepository.UpdateAsync(institution);
                        }
                    }

                    institutionMappings.Remove(institutionMapping);
                }
            }

            if (institutionMappings.Any())
            {
                foreach (var institutionMapping in institutionMappings)
                {
                    var institution = institutions.Find(i => i.Id == institutionMapping.InstitutionId);

                    if (institution?.ActiveVendorId == vendorId)
                    {
                        institution.RecordStatus = RecordStatus.Passive;

                        await _institutionRepository.UpdateAsync(institution);
                    }

                    institutionMapping.RecordStatus = RecordStatus.Passive;

                    await _vendorMapperService.UpdateInstitutionMappingAsync(institutionMapping);
                    await SaveSynchronizationLog(institution, vendorId, SynchronizationType.Disable);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorSyncingInstitutionsForVendor: {vendorId}, Error: {exception}", vendorId, exception);
        }
    }

    private async Task SaveSynchronizationLog(Institution institution, Guid vendorId, SynchronizationType synchronizationType)
    {
        try
        {
            var synchronizationLog = new SynchronizationLog
            {
                ItemId = institution.Id,
                ItemName = institution.Name,
                VendorId = vendorId,
                SynchronizationItem = SynchronizationItem.Institution,
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