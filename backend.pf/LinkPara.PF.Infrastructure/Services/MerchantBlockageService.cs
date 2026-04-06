using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantBlockages;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;
using LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetAllMerchantBlockages;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantBlockageService : IMerchantBlockageService
{
    private readonly ILogger<MerchantBlockageService> _logger;
    private readonly IGenericRepository<MerchantBlockage> _merchantBlockageRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _context;
    public MerchantBlockageService(ILogger<MerchantBlockageService> logger,
        IGenericRepository<MerchantBlockage> merchantBlockageRepository,
        IGenericRepository<Merchant> merchantRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        PfDbContext context)
    {
        _logger = logger;
        _merchantBlockageRepository = merchantBlockageRepository;
        _merchantRepository = merchantRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _context = context;
    }

    public async Task<PaginatedList<MerchantBlockageDto>> GetAllAsync(GetAllMerchantBlockageQuery query)
    {
        var merchantBloackages = _merchantBlockageRepository.GetAll().Include(b => b.Merchant).AsQueryable();

        if (query.MerchantId is not null)
        {
            merchantBloackages = merchantBloackages.Where(b => b.MerchantId == query.MerchantId);
        }

        if (query.MerchantBlockageStatus is not null)
        {
            merchantBloackages = merchantBloackages.Where(b => b.MerchantBlockageStatus == query.MerchantBlockageStatus);
        }

        return await merchantBloackages
       .PaginatedListWithMappingAsync<MerchantBlockage,MerchantBlockageDto>(_mapper, query.Page, query.Size, query.OrderBy, query.SortBy);
    }

    public async Task<MerchantBlockageDto> GetByMerchantIdAsync(Guid merchantId)
    {
        var blockage = await _merchantBlockageRepository.GetAll()
            .Include(b => b.MerchantBlockageDetails).Include(b => b.Merchant)
            .FirstOrDefaultAsync(b => b.MerchantId == merchantId);

        if (blockage is null)
        {
            throw new NotFoundException(nameof(MerchantBlockage));
        }

        return _mapper.Map<MerchantBlockageDto>(blockage);
    }

    public async Task SaveAsync(SaveMerchantBlockageCommand request)
    {
        var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant));
        }

        var merchantCheck = await _merchantBlockageRepository.GetAll()
            .FirstOrDefaultAsync(b => b.MerchantId == request.MerchantId);

        if (merchantCheck is not null)
        {
            throw new AlreadyInUseException(request.MerchantId.ToString());
        }

        try
        {
            var merchantBlockage = _mapper.Map<MerchantBlockage>(request);

            await _merchantBlockageRepository.AddAsync(merchantBlockage);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveMerchantBlockage",
                    SourceApplication = "PF",
                    Resource = "MerchantBlockage",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"MerchantId", request.MerchantId.ToString() },
                        {"RemainingAmount", request.RemainingAmount.ToString() },
                        {"BlockageAmount", request.BlockageAmount.ToString() },
                        {"TotalAmount", request.TotalAmount.ToString() },
                        {"BlockageStatus", request.MerchantBlockageStatus.ToString() }
                    }
                }
            );
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantBlockageSaveError: {exception}");
        }
    }

    public async Task UpdateAsync(UpdateMerchantBlockageCommand request)
    {
        var merchantBlockage = await _merchantBlockageRepository.GetByIdAsync(request.Id);

        if (merchantBlockage is null)
        {
            throw new NotFoundException(nameof(MerchantBlockage));
        }

        if (request.TotalAmount < merchantBlockage.BlockageAmount)
        {
            throw new InvalidParameterException(request.TotalAmount.ToString());
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            merchantBlockage.TotalAmount = request.TotalAmount;
            merchantBlockage.RemainingAmount = request.TotalAmount - merchantBlockage.BlockageAmount;
            merchantBlockage.MerchantBlockageStatus = merchantBlockage.RemainingAmount >0 ? MerchantBlockageStatus.Incomplete : MerchantBlockageStatus.Complete;

            await _merchantBlockageRepository.UpdateAsync(merchantBlockage);

            await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = true,
                  LogDate = DateTime.Now,
                  Operation = "UpdateMerchantBlockage",
                  SourceApplication = "PF",
                  Resource = "MerchantBlockage",
                  UserId = parseUserId,
                  Details = new Dictionary<string, string>
                  {
                        {"Id", request.Id.ToString() },
                        {"TotalAmount", request.TotalAmount.ToString() }
                  }
              });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantBlockageUpdateError: {exception}");
        }
    }

    public async Task UpdatePaymentDateAsync(UpdatePaymentDateCommand request)
    {
        var postingBalance = await _context.PostingBalance.FindAsync(request.PostBalanceId); 

        if (postingBalance is null)
        {
            throw new NotFoundException(nameof(PostingBalance));
        }
        var merchantBlockage = await _context.MerchantBlockage.FindAsync(request.MerchantBlockageId);
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                postingBalance.PaymentDate = request.PaymentDate;
                postingBalance.BlockageStatus = BlockageStatus.Resolved;

                _context.PostingBalance.Update(postingBalance);

                merchantBlockage.RemainingAmount = merchantBlockage.RemainingAmount + postingBalance.TotalAmount;
                merchantBlockage.BlockageAmount = merchantBlockage.BlockageAmount - postingBalance.TotalAmount;
                merchantBlockage.MerchantBlockageStatus = merchantBlockage.RemainingAmount > 0 ? MerchantBlockageStatus.Incomplete : MerchantBlockageStatus.Complete;

                merchantBlockage.RemainingAmount = merchantBlockage.RemainingAmount > merchantBlockage.TotalAmount
                    ? merchantBlockage.TotalAmount
                    : merchantBlockage.RemainingAmount;

                var tempRemainingAmount = merchantBlockage.RemainingAmount + merchantBlockage.BlockageAmount;

                if(tempRemainingAmount > merchantBlockage.TotalAmount)
                {
                    merchantBlockage.RemainingAmount = merchantBlockage.TotalAmount - merchantBlockage.BlockageAmount;
                }

                _context.MerchantBlockage.Update(merchantBlockage);

                var postingTransactions = await _context.PostingTransaction
                .Where(s => s.PostingBalanceId == postingBalance.Id && s.BlockageStatus == BlockageStatus.Blocked)
                .ToListAsync();

                var merchantTransactions = await _context.MerchantTransaction.Where(s =>
                    postingTransactions.Select(a => a.MerchantTransactionId).ToList().Contains(s.Id)).ToListAsync();
                
                postingTransactions.ForEach(s =>
                {
                    s.BlockageStatus = BlockageStatus.Resolved;
                    s.OldPaymentDate = s.PaymentDate;
                    s.PaymentDate = request.PaymentDate;
                });
                
                merchantTransactions.ForEach(s =>
                {
                    s.BlockageStatus = BlockageStatus.Resolved;
                    s.PfPaymentDate = request.PaymentDate;
                });
                
                _context.PostingTransaction.UpdateRange(postingTransactions);
                _context.MerchantTransaction.UpdateRange(merchantTransactions);

                if (postingBalance.ParentMerchantId != Guid.Empty)
                {
                    var additionalTransaction = await _context.PostingAdditionalTransaction
                        .Where(s => s.RelatedPostingBalanceId == postingBalance.Id).FirstOrDefaultAsync();

                    if (additionalTransaction is not null)
                    {
                        var parentMerchantCommissionBalance =
                            await _context.PostingBalance.FirstOrDefaultAsync(s => s.Id == additionalTransaction.PostingBalanceId);
                        
                        if (parentMerchantCommissionBalance is not null)
                        {
                            parentMerchantCommissionBalance.PaymentDate = request.PaymentDate;
                            parentMerchantCommissionBalance.BlockageStatus = BlockageStatus.Resolved;
                            _context.PostingBalance.Update(parentMerchantCommissionBalance);
                            
                            additionalTransaction.OldPaymentDate = additionalTransaction.PaymentDate;
                            additionalTransaction.PaymentDate = request.PaymentDate;
                            additionalTransaction.BlockageStatus = BlockageStatus.Resolved;
                            _context.PostingAdditionalTransaction.Update(additionalTransaction);
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                scope.Complete();
            });

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateMerchantBlockagePaymentDate",
                SourceApplication = "PF",
                Resource = "PostingBalance",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                        {"Id", request.PostBalanceId.ToString() },
                        {"PaymentDate", request.PaymentDate.ToString() }
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantBlockageUpdatePaymentDateError: {exception}");
        }

    }
}
