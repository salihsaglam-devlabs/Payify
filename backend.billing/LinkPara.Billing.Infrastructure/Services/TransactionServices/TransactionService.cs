using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillInfoByConversationId;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillTransactions;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LinkPara.Billing.Infrastructure.Services.TransactionServices;

public class TransactionService : ITransactionService
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public TransactionService(IMapper mapper, IGenericRepository<Transaction> transactionRepository)
    {
        _mapper = mapper;
        _transactionRepository = transactionRepository;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _transactionRepository.AddAsync(transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        await _transactionRepository.UpdateAsync(transaction);
    }

    public async Task<PaginatedList<BillTransactionResponseDto>> GetListAsync(GetBillTransactionsQuery request)
    {
        var transactions = _transactionRepository.GetAll()
            .Include(t => t.Institution.ActiveVendor)
            .Include(t => t.Institution.Sector).AsQueryable();
                

        if (request.InstitutionId != null)
        {
            transactions = transactions.Where(t => t.InstitutionId == request.InstitutionId);
        }

        if (request.SectorId != null)
        {
            transactions = transactions.Where(t => t.Institution.SectorId == request.SectorId);
        }

        if (request.VendorId != null)
        {
            transactions = transactions.Where(t => t.VendorId == request.VendorId);
        }

        if (!string.IsNullOrEmpty(request.PayeeFullName))
        {
            transactions = transactions.Where(t => t.PayeeFullName.ToLower().Contains(request.PayeeFullName.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.BillNumber))
        {
            transactions = transactions.Where(t => t.BillNumber == request.BillNumber);
        }

        if (request.BillDueStartDate != null)
        {
            transactions = transactions.Where(t => t.BillDueDate >= request.BillDueStartDate);
        }

        if (request.BillDueEndDate != null)
        {
            transactions = transactions.Where(t => t.BillDueDate <= request.BillDueEndDate);
        }

        if (request.TransactionStatus != null)
        {
            transactions = transactions.Where(t => t.TransactionStatus == request.TransactionStatus);
        }

        if (request.PaymentStartDate != null)
        {
            transactions = transactions.Where(t => t.PaymentDate >= request.PaymentStartDate);
        }

        if (request.PaymentEndDate != null)
        {
            transactions = transactions.Where(t => t.PaymentDate <= request.PaymentEndDate);
        }

        return await transactions
            .OrderByDescending(t => t.BillDueDate)
            .PaginatedListWithMappingAsync<Transaction,BillTransactionResponseDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<BillTransactionResponseDto> GetBillInfoByConversationIdAsync(GetBillInfoByConversationIdQuery request)
    {
        var transaction = await _transactionRepository.GetAll()
            .Include(t => t.Institution.ActiveVendor)
            .Include(t => t.Institution.Sector)
            .FirstOrDefaultAsync(t => t.ProvisionReferenceId == request.ConversationId);

        if (transaction is null)
        {
            throw new NotFoundException($"TransactionNotFound: {request.ConversationId}");
        }

        var transactionResult = _mapper.Map<BillTransactionResponseDto>(transaction);


        return transactionResult;
    }

    public async Task<Transaction> GetByIdAsync(Guid transactionId)
    {
        var transaction = await _transactionRepository.GetAll()
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction is null)
        {
            throw new NotFoundException($"TransactionNotFound: {transactionId}");
        }

        return transaction;
    }

    public async Task<TransactionStatistics> GetTransactionStatisticsAsync(Guid vendorId, DateTime paymentDate)
    {
        var statistics = await _transactionRepository.GetAll()
            .Where(t => t.PaymentDate == paymentDate
                    && t.VendorId == vendorId)
            .GroupBy(t => new { t.VendorId })
            .Select(g => new TransactionStatistics
            {
                VendorId = g.Key.VendorId,
                PaymentAmount = g.Where(t => t.TransactionStatus == TransactionStatus.Paid).Sum(t => t.BillAmount),
                PaymentCount = g.Count(t => t.TransactionStatus == TransactionStatus.Paid),
                CancellationAmount = g.Where(t => t.TransactionStatus == TransactionStatus.Cancelled).Sum(t => t.BillAmount),
                CancellationCount = g.Count(t => t.TransactionStatus == TransactionStatus.Cancelled),
                StatisticsDate = paymentDate
            })
            .FirstOrDefaultAsync();

        return statistics ?? new TransactionStatistics
        {
            VendorId = vendorId,
            PaymentAmount = 0,
            PaymentCount = 0,
            CancellationAmount = 0,
            CancellationCount = 0,
            StatisticsDate = paymentDate
        };
    }

    public async Task<List<TransactionStatistics>> GetTransactionStatisticsByInstitutionAsync(Guid vendorId, Guid institutionId, DateTime paymentDate)
    {
        return await _transactionRepository.GetAll()
            .Where(t => t.PaymentDate.Date == paymentDate.Date
                    && t.VendorId == vendorId
                    && t.InstitutionId == (institutionId == Guid.Empty ? t.InstitutionId : institutionId)
                    && (
                        t.TransactionStatus == TransactionStatus.Paid
                        || 
                        t.TransactionStatus == TransactionStatus.Cancelled
                       )
             )
            .GroupBy(t => new { t.VendorId, t.InstitutionId })
            .Select(g => new TransactionStatistics
            {
                InstitutionId = g.Key.InstitutionId,
                VendorId = g.Key.VendorId,
                PaymentAmount = g.Where(t => t.TransactionStatus == TransactionStatus.Paid).Sum(t => t.BillAmount),
                PaymentCount = g.Count(t => t.TransactionStatus == TransactionStatus.Paid),
                CancellationAmount = g.Where(t => t.TransactionStatus == TransactionStatus.Cancelled).Sum(t => t.BillAmount),
                CancellationCount = g.Count(t => t.TransactionStatus == TransactionStatus.Cancelled)
            })
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetReconciliableInstitutionTransactionsForVendor(Guid institutionId, Guid vendorId, DateTime paymentDate)
    {
        return await _transactionRepository.GetAll()
            .Where(t => t.PaymentDate.Date == paymentDate.Date
                    && t.InstitutionId == institutionId
                    && t.VendorId == vendorId
                    && 
                    (
                        t.TransactionStatus == TransactionStatus.Paid
                        ||
                        t.TransactionStatus == TransactionStatus.Cancelled
                    )
             )
            .ToListAsync();
    }
}
