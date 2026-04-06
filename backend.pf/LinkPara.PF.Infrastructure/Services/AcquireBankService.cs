using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAllAcquireBank;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services;

public class AcquireBankService : IAcquireBankService
{
    private readonly ILogger<AcquireBankService> _logger;
    private readonly IGenericRepository<AcquireBank> _repository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IGenericRepository<CardLoyalty> _cardLoyaltyRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;

    public AcquireBankService(ILogger<AcquireBankService> logger,
        IGenericRepository<AcquireBank> repository,
        IGenericRepository<Bank> bankRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<CardLoyalty> cardLoyaltyRepository,
        PfDbContext dbContext)
    {
        _logger = logger;
        _repository = repository;
        _bankRepository = bankRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _cardLoyaltyRepository = cardLoyaltyRepository;
        _dbContext = dbContext;
    }

    public async Task DeleteAsync(DeleteAcquireBankCommand request)
    {
        var acquireBank = await _repository.GetByIdAsync(request.Id);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), request.Id);
        }

        try
        {
            acquireBank.RecordStatus = RecordStatus.Passive;
            await _repository.UpdateAsync(acquireBank);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteAcquireBank",
                    SourceApplication = "PF",
                    Resource = "AcquireBank",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"AcquireBankDeleteError : {exception}");
            throw;
        }

    }

    public async Task<AcquireBankDto> GetByIdAsync(Guid id)
    {
        var acquireBank = await _repository.GetAll()
            .Include(b => b.Bank).FirstOrDefaultAsync(b => b.Id == id);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), id);
        }

        return _mapper.Map<AcquireBankDto>(acquireBank);
    }

    public async Task<PaginatedList<AcquireBankDto>> GetListAsync(GetAllAcquireBankQuery request)
    {
        var acquireBankList = _repository.GetAll().Include(b => b.Bank)
            .OrderBy(b => b.Bank.Name).AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            acquireBankList = acquireBankList.Where(b => b.Bank.Name.Contains(request.Q));
        }

        if (request.CreateDateStart is not null)
        {
            acquireBankList = acquireBankList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            acquireBankList = acquireBankList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.RecordStatus is not null)
        {
            acquireBankList = acquireBankList.Where(b => b.RecordStatus
                               == request.RecordStatus);
        }

        return await acquireBankList
            .PaginatedListWithMappingAsync<AcquireBank,AcquireBankDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

    }

    public async Task SaveAsync(SaveAcquireBankCommand request)
    {
        var bank = await _bankRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Code == request.BankCode);

        if (bank is null)
        {
            throw new NotFoundException(nameof(Bank), request.BankCode);
        }

        var activeAcquireBank = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.BankCode == request.BankCode);

        if (activeAcquireBank is not null)
        {
            throw new DuplicateRecordException(nameof(AcquireBank), request.BankCode);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        var acquireBank = new AcquireBank
        {
            AcceptAmex = request.AcceptAmex,
            BankCode = request.BankCode,
            EndOfDayHour = request.EndOfDayHour,
            EndOfDayMinute = request.EndOfDayMinute,
            CardNetwork = request.CardNetwork,
            HasSubmerchantIntegration = request.HasSubmerchantIntegration,
            RestrictOwnCardNotOnUs = request.RestrictOwnCardNotOnUs,
            PaymentGwUrl = request.PaymentGwUrl,
            PaymentGwTradeName = request.PaymentGwTradeName,
            PaymentGwTaxNo = request.PaymentGwTaxNo,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        await _repository.AddAsync(acquireBank);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SaveAcquireBank",
            SourceApplication = "PF",
            Resource = "AcquireBank",
            UserId = parseUserId,
            Details = new Dictionary<string, string>
            {
                {"BankCode", request.BankCode.ToString() },
            }
        });

    }

    public async Task UpdateAsync(UpdateAcquireBankCommand request)
    {
        var bank = await _bankRepository.GetAll()
               .FirstOrDefaultAsync(b => b.Code == request.BankCode);

        if (bank is null)
        {
            throw new NotFoundException(nameof(Bank), request.BankCode);
        }

        var acquireBank = await _repository.GetAll()
           .Include(b => b.Bank).FirstOrDefaultAsync(b => b.Id == request.Id);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), request.Id);
        }

        var activeAcquireBank = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.BankCode == request.BankCode
            && b.Id != request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (activeAcquireBank is not null)
        {
            throw new DuplicateRecordException(nameof(AcquireBank), request.BankCode);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                acquireBank.EndOfDayMinute = request.EndOfDayMinute;
                acquireBank.EndOfDayHour = request.EndOfDayHour;
                acquireBank.AcceptAmex = request.AcceptAmex;
                acquireBank.BankCode = request.BankCode;
                acquireBank.HasSubmerchantIntegration = request.HasSubmerchantIntegration;
                acquireBank.RestrictOwnCardNotOnUs = request.RestrictOwnCardNotOnUs;
                acquireBank.PaymentGwTaxNo = request.PaymentGwTaxNo;
                acquireBank.PaymentGwTradeName = request.PaymentGwTradeName;
                acquireBank.PaymentGwUrl = request.PaymentGwUrl;
                acquireBank.LastModifiedBy = userId;
                acquireBank.UpdateDate = DateTime.Now;

                if (request.CardNetwork != acquireBank.CardNetwork)
                {
                    acquireBank.CardNetwork = request.CardNetwork;

                    var loyalty = await _cardLoyaltyRepository.GetAll()
                        .FirstOrDefaultAsync(c => c.BankCode == request.BankCode);

                    loyalty.Name = request.CardNetwork.ToString();

                    await _cardLoyaltyRepository.UpdateAsync(loyalty);
                }

                await _repository.UpdateAsync(acquireBank);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdateAcquireBank",
                        SourceApplication = "PF",
                        Resource = "AcquireBank",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        { "Id", request.Id.ToString() }
                        }
                    });

                transaction.Complete();
            }
            catch (Exception exception)
            {
                _logger.LogError($"AcquireBankUpdateError : {exception}");
                throw;
            }

        });
    }
}
