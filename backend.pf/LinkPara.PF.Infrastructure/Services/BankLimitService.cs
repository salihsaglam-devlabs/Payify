using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.BankLimits;
using LinkPara.PF.Application.Features.BankLimits;
using LinkPara.PF.Application.Features.BankLimits.Command.DeleteBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.UpdateBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Queries.GetAllBankLimit;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services;

public class BankLimitService : IBankLimitService
{
    private readonly ILogger<BankLimitService> _logger;
    private readonly IGenericRepository<BankLimit> _bankLimitRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IBus _bus;
    private readonly IStringLocalizer _notificationLocalizer;
    public BankLimitService(ILogger<BankLimitService> logger,
        IGenericRepository<BankLimit> bankLimitRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IBus bus,
        IContextProvider contextProvider,
        IStringLocalizerFactory factory)
    {
        _logger = logger;
        _bankLimitRepository = bankLimitRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _bus = bus;
        _notificationLocalizer = factory.Create("Notifications", "LinkPara.PF.API");
    }

    public async Task DecrementLimitAsync(UpdateBankLimitRequest request)
    {
        var allBankLimits = await _bankLimitRepository
            .GetAll()
            .Where(b => b.AcquireBankId == request.AcquireBankId
                     && b.RecordStatus == RecordStatus.Active
                     && !b.IsExpired)
            .ToListAsync();
        try
        {

            if (allBankLimits.Any())
            {
                foreach (var item in allBankLimits)
                {
                    if (item.BankLimitType == BankLimitType.AllTransaction)
                    {
                        item.TotalAmount -= request.Amount;
                    }

                    if (item.BankLimitType == request.BankLimitType)
                    {
                        item.TotalAmount -= request.Amount;
                    }

                    if (request.BankLimitType == BankLimitType.Installment)
                    {
                        if (item.BankLimitType == BankLimitType.OnUs)
                        {
                            item.TotalAmount -= request.Amount;
                        }
                    }
                }

                await _bankLimitRepository.UpdateRangeAsync(allBankLimits);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"DecrementLimitError : {exception}");
            throw;
        }
    }

    public async Task DeleteAsync(DeleteBankLimitCommand request)
    {
        var bankLimit = await _bankLimitRepository.GetByIdAsync(request.Id);

        if (bankLimit is null)
        {
            throw new NotFoundException(nameof(BankLimit), request.Id);
        }

        try
        {
            bankLimit.RecordStatus = RecordStatus.Passive;
            await _bankLimitRepository.UpdateAsync(bankLimit);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteBankLimit",
                    SourceApplication = "PF",
                    Resource = "BankLimit",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"BankLimitDeleteError : {exception}");
            throw;
        }
    }

    public async Task<BankLimitDto> GetByIdAsync(Guid id)
    {
        var bankLimit = await _bankLimitRepository.GetAll()
            .Include(b => b.AcquireBank)
            .ThenInclude(c => c.Bank)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bankLimit is null)
        {
            throw new NotFoundException(nameof(BankLimit), id);
        }

        return _mapper.Map<BankLimitDto>(bankLimit);
    }

    public async Task<PaginatedList<BankLimitDto>> GetListAsync(GetAllBankLimitQuery request)
    {
        var bankLimits = _bankLimitRepository.GetAll()
                         .Include(b => b.AcquireBank)
                         .ThenInclude(b => b.Bank)
                         .AsQueryable();

        if (request.AcquireBankId is not null)
        {
            bankLimits = bankLimits.Where(b => b.AcquireBankId == request.AcquireBankId);
        }

        if (request.CreateDateStart is not null)
        {
            bankLimits = bankLimits.Where(b => b.CreateDate >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            bankLimits = bankLimits.Where(b => b.CreateDate <= request.CreateDateEnd);
        }

        if (request.MonthlyLimitAmount is not null)
        {
            bankLimits = bankLimits.Where(b => b.MonthlyLimitAmount == request.MonthlyLimitAmount);
        }

        if (request.BankLimitType is not null)
        {
            bankLimits = bankLimits.Where(b => b.BankLimitType == request.BankLimitType);
        }

        if (request.LastValidDate is not null)
        {
            bankLimits = bankLimits.Where(b => b.LastValidDate == request.LastValidDate);
        }

        if (request.RecordStatus is not null)
        {
            bankLimits = bankLimits.Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.IsExpired is not null)
        {
            bankLimits = bankLimits.Where(b => b.IsExpired == request.IsExpired);
        }

        return await bankLimits.OrderBy(a => a.AcquireBank.Bank.Name)
            .PaginatedListWithMappingAsync<BankLimit, BankLimitDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task IncrementLimitAsync(UpdateBankLimitRequest request)
    {
        var allBankLimits = await _bankLimitRepository
            .GetAll()
            .Include(b => b.AcquireBank)
            .ThenInclude(c => c.Bank)
            .Where(b => b.AcquireBankId == request.AcquireBankId
                     && b.RecordStatus == RecordStatus.Active
                     && !b.IsExpired)
            .ToListAsync();
        try
        {

            if (allBankLimits.Any())
            {
                foreach (var item in allBankLimits)
                {
                    if (item.BankLimitType == BankLimitType.AllTransaction)
                    {
                        item.TotalAmount += request.Amount;
                    }

                    if (item.BankLimitType == request.BankLimitType)
                    {
                        item.TotalAmount += request.Amount;
                    }

                    if (request.BankLimitType == BankLimitType.Installment)
                    {
                        if (item.BankLimitType == BankLimitType.OnUs)
                        {
                            item.TotalAmount += request.Amount;
                        }
                    }

                    var calculate = CalculeteAmount(item.TotalAmount, item.MonthlyLimitAmount, item.MarginRatio);

                    if (calculate == true)
                    {
                        var bankLimitType = _notificationLocalizer.GetString(item.BankLimitType.ToString()).Value;
                        await SendToBusAsync(item.AcquireBank.Bank.Name, bankLimitType, item.TotalAmount.ToString("0.00"), item.MonthlyLimitAmount.ToString("0.00"));
                    }
                }

                await _bankLimitRepository.UpdateRangeAsync(allBankLimits);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"IncrementLimitError : {exception}");
            throw;
        }
    }
    private static bool CalculeteAmount(decimal totalAmount, decimal monthlyAmount, int marginRatio)
    {
        var remaningLimit = monthlyAmount - (monthlyAmount * marginRatio / 100);

        if (totalAmount >= remaningLimit)
        {
            return true;
        }

        return false;
    }
    private async Task SendToBusAsync(string bank, string bankLimitType, string totalLimit, string usedLimit)
    {
        await _bus.Publish(new SharedModels.Notification.NotificationModels.PF.BankLimit
        {
            BankName = bank,
            BankLimitType = bankLimitType,
            TotalBankLimit = totalLimit,
            UsedBankLimit = usedLimit
        });
    }

    public async Task SaveAsync(SaveBankLimitCommand request)
    {
        var bankLimitControl = await _bankLimitRepository.GetAll()
            .Where(b => b.AcquireBankId == request.AcquireBankId && 
                   b.BankLimitType == request.BankLimitType && 
                   b.RecordStatus == RecordStatus.Active &&
                   !b.IsExpired)
            .FirstOrDefaultAsync();

        if (bankLimitControl is not null)
        {
            throw new DuplicateRecordException(nameof(BankLimit), request.AcquireBankId);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        var bankLimit = new BankLimit
        {
            AcquireBankId = request.AcquireBankId,
            BankLimitType = request.BankLimitType,
            MarginRatio = request.MarginRatio,
            MonthlyLimitAmount = request.MonthlyLimitAmount,
            TotalAmount = 0M,
            LastValidDate = request.LastValidDate,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        await _bankLimitRepository.AddAsync(bankLimit);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SaveBankLimit",
            SourceApplication = "PF",
            Resource = "BankLimit",
            UserId = parseUserId,
            Details = new Dictionary<string, string>
            {
                  {"AcquireBankId", request.AcquireBankId.ToString() },
            }
        });
    }

    public async Task UpdateAsync(UpdateBankLimitCommand request)
    {
        var bankLimit = await _bankLimitRepository
            .GetAll()
            .Include(b => b.AcquireBank)
            .ThenInclude(c => c.Bank)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (bankLimit is null)
        {
            throw new NotFoundException(nameof(BankLimit), request.Id);
        }

        if (request.MonthlyLimitAmount < bankLimit.TotalAmount)
        {
            throw new BankLimitExceededException();
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            bankLimit.MonthlyLimitAmount = request.MonthlyLimitAmount;
            bankLimit.MarginRatio = request.MarginRatio;
            bankLimit.LastValidDate = request.LastValidDate;
            bankLimit.LastModifiedBy = userId;
            bankLimit.UpdateDate = DateTime.Now;

            await _bankLimitRepository.UpdateAsync(bankLimit);

            var calculate = CalculeteAmount(bankLimit.TotalAmount, bankLimit.MonthlyLimitAmount, bankLimit.MarginRatio);

            if (calculate == true)
            {
                var bankLimitType = _notificationLocalizer.GetString(bankLimit.BankLimitType.ToString()).Value;
                await SendToBusAsync(bankLimit.AcquireBank.Bank.Name, bankLimitType, bankLimit.TotalAmount.ToString("0.00"), bankLimit.MonthlyLimitAmount.ToString("0.00"));
            }

            await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = true,
                   LogDate = DateTime.Now,
                   Operation = "UpdateBankLimit",
                   SourceApplication = "PF",
                   Resource = "BankLimit",
                   UserId = parseUserId,
                   Details = new Dictionary<string, string>
                   {
                    {"Id", request.Id.ToString() },
                   }
               });

        }
        catch (Exception exception)
        {
            _logger.LogError($"BankLimitUpdateError : {exception}");
            throw;
        }
    }
}
