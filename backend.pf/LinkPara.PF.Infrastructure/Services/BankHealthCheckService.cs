using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.BankHealthChecks;
using LinkPara.PF.Application.Features.BankHealthChecks;
using LinkPara.PF.Application.Features.BankHealthChecks.Command.UpdateBankHealthCheck;
using LinkPara.PF.Application.Features.BankHealthChecks.Queries.GetAllBankHealthCheck;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class BankHealthCheckService : IBankHealthCheckService
{
    private readonly ILogger<BankHealthCheckService> _logger;
    private readonly IGenericRepository<BankHealthCheck> _repository;
    private readonly IGenericRepository<AcquireBank> _bankRepository;
    private readonly IGenericRepository<BankHealthCheckTransaction> _bankHealthCheckTransationRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IVaultClient _vaultClient;
    private readonly IBus _bus;
    private readonly IStringLocalizer _notificationLocalizer;

    public BankHealthCheckService(ILogger<BankHealthCheckService> logger,
        IGenericRepository<BankHealthCheck> repository,
        IGenericRepository<AcquireBank> bankRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IVaultClient vaultClient,
        IBus bus,
        IStringLocalizerFactory factory,
        IGenericRepository<BankHealthCheckTransaction> bankHealthCheckTransationRepository)
    {
        _logger = logger;
        _repository = repository;
        _bankRepository = bankRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _vaultClient = vaultClient;
        _bus = bus;
        _notificationLocalizer = factory.Create("Notifications", "LinkPara.PF.API");
        _bankHealthCheckTransationRepository = bankHealthCheckTransationRepository;
    }

    public async Task<PaginatedList<BankHealthCheckDto>> GetListAsync(GetAllBankHealthCheckQuery request)
    {
        var bankHealthChecks = _repository.GetAll()
                          .Include(b => b.AcquireBank)
                          .ThenInclude(b => b.Bank)
                          .AsQueryable();

        if (request.AcquireBankId is not null)
        {
            bankHealthChecks = bankHealthChecks.Where(b => b.AcquireBankId == request.AcquireBankId);
        }

        if (request.LastCheckDateStart is not null)
        {
            bankHealthChecks = bankHealthChecks.Where(b => b.LastCheckDate >= request.LastCheckDateStart);
        }

        if (request.LastCheckDateEnd is not null)
        {
            bankHealthChecks = bankHealthChecks.Where(b => b.LastCheckDate <= request.LastCheckDateEnd);
        }

        if (request.HealthCheckType is not null)
        {
            bankHealthChecks = bankHealthChecks.Where(b => b.HealthCheckType == request.HealthCheckType);
        }

        return await bankHealthChecks.OrderBy(a => a.AcquireBank.Bank.Name)
            .PaginatedListWithMappingAsync<BankHealthCheck, BankHealthCheckDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task UpdateAsync(UpdateBankHealthCheckCommand request)
    {
        var bankHealthCheck = await _repository
            .GetAll()
            .Include(b => b.AcquireBank)
            .ThenInclude(b => b.Bank)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (bankHealthCheck is null)
        {
            throw new NotFoundException(nameof(BankHealthCheck), request.Id);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            bankHealthCheck.IsHealthCheckAllowed = request.IsHealthCheckAllowed;
            bankHealthCheck.LastModifiedBy = userId;
            bankHealthCheck.UpdateDate = DateTime.Now;

            var oldCheckType = bankHealthCheck.HealthCheckType;

            if (bankHealthCheck.IsHealthCheckAllowed == false)
            {
                bankHealthCheck.HealthCheckType = HealthCheckType.OutOfControl;
            }
            else
            {
                bankHealthCheck.HealthCheckType = HealthCheckType.Pending;
                bankHealthCheck.AllowedCheckDate = DateTime.Now;
                bankHealthCheck.LastCheckDate = DateTime.Now;
                bankHealthCheck.FailTransactionCount = 0;
                bankHealthCheck.TotalTransactionCount = 0;
                bankHealthCheck.FailTransactionRate = 0;
            }

            var newCheckType = bankHealthCheck.HealthCheckType;

            if (oldCheckType != newCheckType)
            {
                await SendToBusAsync(bankHealthCheck.AcquireBank.Bank.Name, oldCheckType, newCheckType, bankHealthCheck.FailTransactionRate);
            }

            await _repository.UpdateAsync(bankHealthCheck);

            await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = true,
                   LogDate = DateTime.Now,
                   Operation = "UpdateBankHealthCheck",
                   SourceApplication = "PF",
                   Resource = "BankHealthCheck",
                   UserId = parseUserId,
                   Details = new Dictionary<string, string>
                   {
                    {"Id", request.Id.ToString() },
                   }
               });

        }
        catch (Exception exception)
        {
            _logger.LogError($"BankHealthCheckUpdateError : {exception}");
            throw;
        }
    }

    public async Task UpdateHealthCheckAsync(Guid acquireBankId)
    {
        try
        {
            var isHealthCheckEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "HealthCheckEnabled");

            if (!isHealthCheckEnabled)
            {
                return;
            }
        }
        catch (Exception e)
        {
            return;
        }

        var bankHealthCheck = await _repository
                            .GetAll()
                            .Include(b => b.AcquireBank)
                            .ThenInclude(c => c.Bank)
                            .FirstOrDefaultAsync(b => b.AcquireBankId == acquireBankId);

        var acquireBank = await _bankRepository.GetAll()
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == acquireBankId);

        var healthChechkModel = _vaultClient.GetSecretValue<BankHealthCheckInfoModel>("PFSecrets", "BankHealthCheckInfo", null);

        var doubleMinute = Convert.ToDouble(healthChechkModel.RangeMinute);
        var date = DateTime.Now.AddMinutes(-doubleMinute);

        if (bankHealthCheck is not null && date < bankHealthCheck.AllowedCheckDate)
        {
            date = bankHealthCheck.AllowedCheckDate;
        }

        var bankHealthCheckTransactions = await _bankHealthCheckTransationRepository
                                  .GetAll()
                                  .Where(b => b.BankTransactionDate >= date && b.AcquireBankCode == acquireBank.BankCode)
                                  .ToListAsync();

        var totalTransactionCount = 0;
        var failTransactionCount = 0;
        decimal failRate = 0;

        if (bankHealthCheckTransactions.Any())
        {
            totalTransactionCount = bankHealthCheckTransactions.Count;
            failTransactionCount = bankHealthCheckTransactions.Where(b => b.TransactionStatus != TransactionStatus.Success).ToList().Count;
            failRate = (Convert.ToDecimal(failTransactionCount) / Convert.ToDecimal(totalTransactionCount)) * 100;
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (totalTransactionCount >= healthChechkModel.MinTransactionCount)
        {
            if (bankHealthCheck is null)
            {
                var addHealthCheck = new BankHealthCheck
                {
                    AcquireBankId = acquireBankId,
                    LastCheckDate = DateTime.Now,
                    AllowedCheckDate = DateTime.Now,
                    TotalTransactionCount = totalTransactionCount,
                    FailTransactionCount = failTransactionCount,
                    FailTransactionRate = Convert.ToInt32(failRate),
                    HealthCheckType = HealthCheckType.Healthy,
                    IsHealthCheckAllowed = true,
                    CreatedBy = parseUserId.ToString(),
                    CreateDate = DateTime.Now
                };

                if (failRate >= healthChechkModel.FailTransactionRate)
                {
                    addHealthCheck.HealthCheckType = HealthCheckType.Unhealthy;

                    await SendToBusAsync(acquireBank.Bank.Name, HealthCheckType.Healthy, addHealthCheck.HealthCheckType, Convert.ToInt32(failRate));
                }

                await _repository.AddAsync(addHealthCheck);

                return;
            }

            bankHealthCheck.LastCheckDate = DateTime.Now;
            bankHealthCheck.TotalTransactionCount = totalTransactionCount;
            bankHealthCheck.FailTransactionCount = failTransactionCount;
            bankHealthCheck.FailTransactionRate = Convert.ToInt32(failRate);
            bankHealthCheck.LastModifiedBy = parseUserId.ToString();
            bankHealthCheck.UpdateDate = DateTime.Now;

            if (bankHealthCheck.HealthCheckType != HealthCheckType.OutOfControl)
            {
                var oldCheckType = bankHealthCheck.HealthCheckType;
                if (failRate >= healthChechkModel.FailTransactionRate)
                {
                    bankHealthCheck.HealthCheckType = HealthCheckType.Unhealthy;
                }
                else
                {
                    bankHealthCheck.HealthCheckType = HealthCheckType.Healthy;
                }

                if (oldCheckType != bankHealthCheck.HealthCheckType)
                {
                    await SendToBusAsync(bankHealthCheck.AcquireBank.Bank.Name, oldCheckType, bankHealthCheck.HealthCheckType, Convert.ToInt32(failRate));
                }

            }

            await _repository.UpdateAsync(bankHealthCheck);
        }
        else
        {
            if (bankHealthCheck is null)
            {
                var addHealthCheck = new BankHealthCheck
                {
                    AcquireBankId = acquireBankId,
                    LastCheckDate = DateTime.Now,
                    AllowedCheckDate = DateTime.Now,
                    TotalTransactionCount = totalTransactionCount,
                    FailTransactionCount = failTransactionCount,
                    FailTransactionRate = Convert.ToInt32(failRate),
                    HealthCheckType = HealthCheckType.Pending,
                    IsHealthCheckAllowed = true,
                    CreatedBy = parseUserId.ToString(),
                    CreateDate = DateTime.Now
                };

                await _repository.AddAsync(addHealthCheck);

                return;
            }
            else
            {
                var oldCheckType = bankHealthCheck.HealthCheckType;
                if (bankHealthCheck.IsHealthCheckAllowed == false)
                {
                    bankHealthCheck.HealthCheckType = HealthCheckType.OutOfControl;
                }
                var newCheckType = bankHealthCheck.HealthCheckType;

                if (oldCheckType != newCheckType)
                {
                    await SendToBusAsync(bankHealthCheck.AcquireBank.Bank.Name, oldCheckType, newCheckType, Convert.ToInt32(failRate));
                }

                bankHealthCheck.LastCheckDate = DateTime.Now;
                bankHealthCheck.TotalTransactionCount = totalTransactionCount;
                bankHealthCheck.FailTransactionCount = failTransactionCount;
                bankHealthCheck.FailTransactionRate = Convert.ToInt32(failRate);
                bankHealthCheck.LastModifiedBy = parseUserId.ToString();
                bankHealthCheck.UpdateDate = DateTime.Now;

                await _repository.UpdateAsync(bankHealthCheck);
            }
        }
    }
    private async Task SendToBusAsync(string bank, HealthCheckType oldCheckType, HealthCheckType newCheckType, int failRate)
    {
        var stringFailRate = $"%{failRate.ToString()}";
        var localizeNewCheckType = _notificationLocalizer.GetString(newCheckType.ToString()).Value;
        var localizeOldCheckType = _notificationLocalizer.GetString(oldCheckType.ToString()).Value;

        await _bus.Publish(new SharedModels.Notification.NotificationModels.PF.BankHealthCheck
        {
            BankName = bank,
            FailRate = stringFailRate,
            NewCheckType = localizeNewCheckType,
            OldCheckType = localizeOldCheckType
        });
    }
}
