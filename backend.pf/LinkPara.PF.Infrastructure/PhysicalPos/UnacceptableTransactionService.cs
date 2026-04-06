using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.UpdateUnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetAllUnacceptableTransaction;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.PhysicalPos;

public class UnacceptableTransactionService : IUnacceptableTransactionService
{
    private readonly IPaxPosService _paxPosService;
    private readonly IGenericRepository<PhysicalPosUnacceptableTransaction> _repository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<PhysicalPosEndOfDay> _physicalPosEndOfDayRepository;
    private readonly IGenericRepository<PhysicalPosReconciliationTransaction> _reconciliationTransactionRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public UnacceptableTransactionService(
        IPaxPosService paxPosService,
        IGenericRepository<PhysicalPosUnacceptableTransaction> repository,
        IMapper mapper, 
        IGenericRepository<MerchantTransaction> merchantTransactionRepository, 
        IGenericRepository<PhysicalPosEndOfDay> physicalPosEndOfDayRepository, 
        IGenericRepository<PhysicalPosReconciliationTransaction> reconciliationTransactionRepository, 
        IAuditLogService auditLogService)
    {
        _paxPosService = paxPosService;
        _repository = repository;
        _mapper = mapper;
        _merchantTransactionRepository = merchantTransactionRepository;
        _physicalPosEndOfDayRepository = physicalPosEndOfDayRepository;
        _reconciliationTransactionRepository = reconciliationTransactionRepository;
        _auditLogService = auditLogService;
    }

    public async Task RetryUnacceptableTransactionAsync(Guid unacceptableTransactionId)
    {
        var unacceptableTransaction = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == unacceptableTransactionId);

        if (unacceptableTransaction is null)
        {
            throw new NotFoundException(nameof(PhysicalPosUnacceptableTransaction), unacceptableTransactionId);
        }

        if (unacceptableTransaction.CurrentStatus != UnacceptableTransactionStatus.ActionRequired)
        {
            throw new InvalidUnacceptableStatusException();
        }

        if (string.Equals(unacceptableTransaction.Vendor, nameof(PhysicalPosVendor.Pax),
                StringComparison.CurrentCultureIgnoreCase))
        {
            await _paxPosService.RetryTransactionAsync(unacceptableTransaction);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<PaginatedList<PhysicalPosUnacceptableTransactionDto>> GetAllUnacceptableTransactionsAsync(
        GetAllUnacceptableTransactionQuery request)
    {
        var unacceptableTransactionList = _repository.GetAll().AsQueryable();

        if (request.PaymentId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList
                .Where(b => b.PaymentId.Contains(request.PaymentId, StringComparison.CurrentCultureIgnoreCase));
        }

        if (request.BatchId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList
                .Where(b => b.BatchId == request.BatchId);
        }

        if (request.DateStart is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Date
                >= request.DateStart);
        }

        if (request.DateEnd is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Date
                <= request.DateEnd);
        }

        if (request.Type is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList
                .Where(b => b.Type == request.Type);
        }

        if (request.Status is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Status == request.Status);
        }

        if (request.Currency is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Currency == request.Currency);
        }

        if (request.MerchantId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.TerminalId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.TerminalId == request.TerminalId);
        }

        if (request.BinNumber is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.BinNumber == request.BinNumber);
        }

        if (request.ProvisionNo is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.ProvisionNo == request.ProvisionNo);
        }

        if (request.Vendor is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Vendor == request.Vendor);
        }

        if (request.Rrn is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Rrn == request.Rrn);
        }

        if (request.Stan is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.Stan == request.Stan);
        }

        if (request.BankRef is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.BankRef == request.BankRef);
        }

        if (request.OriginalRef is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.OriginalRef == request.OriginalRef);
        }

        if (request.PfMerchantId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.PfMerchantId == request.PfMerchantId);
        }

        if (request.ConversationId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.ConversationId == request.ConversationId);
        }

        if (request.SerialNumber is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.SerialNumber == request.SerialNumber);
        }

        if (request.CurrentStatus is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.CurrentStatus == request.CurrentStatus);
        }

        if (request.PhysicalPosEodId is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.PhysicalPosEodId == request.PhysicalPosEodId);
        }

        if (request.EndOfDayStatus is not null)
        {
            unacceptableTransactionList = unacceptableTransactionList.Where(b => b.EndOfDayStatus == request.EndOfDayStatus);
        }


        return await unacceptableTransactionList
            .PaginatedListWithMappingAsync<PhysicalPosUnacceptableTransaction, PhysicalPosUnacceptableTransactionDto>(
                _mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<UnacceptableTransactionDetailResponse> GetDetailsByIdAsync(Guid id)
    {
        var unacceptableTransaction = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (unacceptableTransaction is null)
        {
            throw new NotFoundException(nameof(PhysicalPosUnacceptableTransaction), id);
        }

        var response = new UnacceptableTransactionDetailResponse
        {
            UnacceptableTransaction = _mapper.Map<PhysicalPosUnacceptableTransactionDto>(unacceptableTransaction)
        };

        if (unacceptableTransaction.MerchantTransactionId != Guid.Empty)
        {
            var relatedMerchantTransaction = await _merchantTransactionRepository
                .GetAll()
                .Where(s => s.Id == unacceptableTransaction.MerchantTransactionId)
                .FirstOrDefaultAsync();
            if (relatedMerchantTransaction is not null)
            {
                response.RelatedMerchantTransaction =
                    _mapper.Map<MerchantTransactionDto>(relatedMerchantTransaction);
            }
        }
        
        if (unacceptableTransaction.PhysicalPosEodId != Guid.Empty)
        {
            var relatedEndOfDay = await _physicalPosEndOfDayRepository
                .GetAll()
                .Where(s => s.Id == unacceptableTransaction.PhysicalPosEodId)
                .FirstOrDefaultAsync();
            if (relatedEndOfDay is not null)
            {
                response.RelatedEndOfDay = _mapper.Map<PhysicalPosEndOfDayDto>(relatedEndOfDay);
            }
        }
        
        var relatedReconciliationTransaction = await _reconciliationTransactionRepository
            .GetAll()
            .Where(s => s.RecordStatus == RecordStatus.Active && s.UnacceptableTransactionId == id)
            .FirstOrDefaultAsync();
        if (relatedReconciliationTransaction is not null)
        {
            response.RelatedReconciliationTransaction =
                _mapper.Map<ReconciliationTransactionDto>(relatedReconciliationTransaction);
        }

        return response;
    }

    public async Task<PhysicalPosUnacceptableTransactionDto> UpdateStatusAsync(UpdateUnacceptableTransactionCommand command)
    {
        var unacceptableTransaction = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == command.Id);

        if (unacceptableTransaction is null)
        {
            throw new NotFoundException(nameof(PhysicalPosUnacceptableTransaction), command.Id);
        }
        
        var oldStatus = unacceptableTransaction.CurrentStatus;

        var unacceptableTransactionMap = _mapper.Map<UpdateUnacceptableTransactionRequest>(unacceptableTransaction);
        command.UnacceptableTransaction.ApplyTo(unacceptableTransactionMap);
        
        if (oldStatus is not UnacceptableTransactionStatus.ActionRequired || 
            (unacceptableTransactionMap.CurrentStatus is not UnacceptableTransactionStatus.ManuallyHandled and UnacceptableTransactionStatus.Rejected))
        {
            throw new InvalidUnacceptableStatusException();
        }
        
        _mapper.Map(unacceptableTransactionMap, unacceptableTransaction);
        await _repository.UpdateAsync(unacceptableTransaction);
        
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateUnacceptableTransaction",
                SourceApplication = "PF",
                Resource = "PhysicalPosUnacceptableTransaction",
                Details = new Dictionary<string, string>
                {
                    { "Id", unacceptableTransaction.Id.ToString() },
                    { "OldStatus", oldStatus.ToString() },
                    { "CurrentStatus", unacceptableTransactionMap.CurrentStatus.ToString() },
                }
            });

        return _mapper.Map<PhysicalPosUnacceptableTransactionDto>(unacceptableTransaction);
    }
}