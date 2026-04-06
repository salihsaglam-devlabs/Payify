using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Reconciliations;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionDetail;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionSummary;
using LinkPara.Billing.Application.Features.Reconciliations.Queries.GetSummary;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Infrastructure.Services.ReconciliationServices;

public class ReconcilationService : IReconciliationService
{
    private readonly IGenericRepository<InstitutionSummary> _institutionSummaryRepository;
    private readonly IGenericRepository<InstitutionDetail> _institutionDetailRepository;
    private readonly IGenericRepository<Summary> _summaryRepository;

    public ReconcilationService(IGenericRepository<InstitutionSummary> institutionSummaryRepository,
        IGenericRepository<InstitutionDetail> institutionDetailRepository,
        IGenericRepository<Summary> summaryRepository)
    {
        _institutionSummaryRepository = institutionSummaryRepository;
        _institutionDetailRepository = institutionDetailRepository;
        _summaryRepository = summaryRepository;
    }

    public async Task<PaginatedList<InstitutionSummaryDto>> GetInstitutionSummaryAsync(GetInstitutionSummaryQuery request)
    {
        var summaries = _institutionSummaryRepository.GetAll()
            .Include(i => i.Institution)
            .ThenInclude(i => i.Sector)
            .Include(i => i.Vendor)
            .Where(i => i.ReconciliationDate >= request.StartTime 
                && i.ReconciliationDate <= request.EndTime
                && i.RecordStatus == RecordStatus.Active
            );

        if (request.InstitutionId != null && request.InstitutionId != Guid.Empty)
        {
            summaries = summaries.Where(i => i.InstitutionId == request.InstitutionId);
        }
        
        if (request.VendorId != null && request.VendorId != Guid.Empty)
        {
            summaries = summaries.Where(i => i.VendorId == request.VendorId);
        }

        if (request.ReconciliationStatus != null)
        {
            summaries = summaries.Where(i => i.ReconciliationStatus == request.ReconciliationStatus);
        }

        return await summaries.Select(i => new InstitutionSummaryDto
        {
            Id = i.Id,
            VendorId = i.VendorId,
            VendorName = i.Vendor.Name,
            SectorName = i.Institution.Sector.Name,
            InstitutionId = i.InstitutionId,
            InstitutionName = i.Institution.Name,
            TotalPaymentAmount = i.TotalPaymentAmount,
            TotalPaymentCount = i.TotalPaymentCount,
            TotalCancelAmount = i.TotalCancelAmount,
            TotalCancelCount = i.TotalCancelCount,
            VendorTotalPaymentAmount = i.VendorTotalPaymentAmount,
            VendorTotalPaymentCount = i.VendorTotalPaymentCount,
            VendorTotalCancelAmount = i.VendorTotalCancelAmount,
            VendorTotalCancelCount = i.VendorTotalCancelCount,
            ReconciliationDate = i.ReconciliationDate,
            ReconciliationStatus = i.ReconciliationStatus,
            CanRetryReconciliation = i.ReconciliationStatus == ReconciliationStatus.Fail
                                        && 
                                        (
                                            i.TotalPaymentCount >= i.VendorTotalPaymentCount
                                            || 
                                            i.TotalCancelCount >= i.VendorTotalCancelCount
                                        )
        })
        .OrderByDescending(o => o.ReconciliationDate)
        .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<PaginatedList<InstitutionDetailDto>> GetInstitutionDetailAsync(GetInstitutionDetailQuery request)
    {
        return await _institutionDetailRepository
            .GetAll()
            .Include(i => i.Transaction)
            .Where(i => i.InstitutionSummaryId == request.InstitutionSummaryId
                && i.RecordStatus == RecordStatus.Active
                && i.PaymentStatus != PaymentStatus.CancelledTransaction
            )
            .Select(d => new InstitutionDetailDto
            {
                TransactionId = d.TransactionId,
                BillAmount = d.BillAmount,
                BillCurrency = d.BillCurrency,
                BillDate = d.BillDate != DateTime.MinValue ? d.BillDate : null,
                BillDueDate = d.BillDueDate != DateTime.MinValue ? d.BillDueDate : null,
                BillNumber = d.BillNumber,
                BillPaymentDate = d.Transaction != null ? d.Transaction.PaymentDate : null,
                ReferenceId = d.Transaction != null ? d.Transaction.ReferenceId : string.Empty,
                PaymentStatus = d.PaymentStatus,
                ReconciliationDate = d.ReconciliationDate,
                SubscriberName = d.Transaction != null ? d.Transaction.SubscriberName : string.Empty,
                SubscriberNumber1 = d.Transaction != null ? d.Transaction.SubscriptionNumber1 : string.Empty,
                SubscriberNumber2 = d.Transaction != null ? d.Transaction.SubscriptionNumber2 : string.Empty,
                SubscriberNumber3 = d.Transaction != null ? d.Transaction.SubscriptionNumber3 : string.Empty,
                CanCancelTransaction = d.PaymentStatus == PaymentStatus.MissingVendorTransaction,
                VendorExistingTransaction = d.PaymentStatus == PaymentStatus.ExistBothSides || d.PaymentStatus == PaymentStatus.MissingTransaction,
                ExistingTransaction = d.PaymentStatus == PaymentStatus.ExistBothSides || d.PaymentStatus == PaymentStatus.MissingVendorTransaction

            })
            .OrderByDescending(o => o.ReconciliationDate)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<InstitutionSummary> GetInstitutionSummaryByIdAsync(Guid institutionSummaryId)
    {
        var institutionSummary = await _institutionSummaryRepository
            .GetAll()
            .Include(i => i.Vendor)
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(w => w.Id == institutionSummaryId);

        if (institutionSummary == null)
        {
            throw new NotFoundException($"InstitutionSummaryNotFoundForId: {institutionSummaryId}");
        }

        return institutionSummary;
    }

    public async Task<PaginatedList<SummaryDto>> GetSummaryAsync(GetSummaryQuery request)
    {
        var summaries = _summaryRepository
            .GetAll()
            .Include(i => i.Vendor)
            .Where(w => w.ReconciliationDate >= request.ReconciliationStartDate
                && w.ReconciliationDate <= request.ReconciliationEndDate
                && w.RecordStatus == RecordStatus.Active
            );

        if (request.VendorId != null)
        {
            summaries = summaries.Where(w => w.VendorId == request.VendorId);
        }

        if (request.ReconciliationStatus != null)
        {
            summaries = summaries.Where(w => w.ReconciliationStatus == request.ReconciliationStatus);
        }

        var response = await summaries
            .Select(s => new SummaryDto
            {
                VendorId = s.VendorId,
                VendorName = s.Vendor.Name,
                TotalPaymentAmount = s.TotalPaymentAmount,
                TotalCancelAmount = s.TotalCancelAmount,
                TotalPaymentCount = s.TotalPaymentCount,
                TotalCancelCount = s.TotalCancelCount,
                VendorTotalPaymentAmount = s.VendorTotalPaymentAmount,
                VendorTotalCancelAmount = s.VendorTotalCancelAmount,
                VendorTotalPaymentCount = s.VendorTotalPaymentCount,
                VendorTotalCancelCount = s.VendorTotalCancelCount,
                ReconciliationDate = s.ReconciliationDate,
                ReconciliationStatus = s.ReconciliationStatus,
                CanRetryReconciliation = s.ReconciliationStatus != ReconciliationStatus.Success,
                Explanation = s.Explanation
            })
            .OrderByDescending(o => o.ReconciliationDate)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);

        var institutionSummaries = await _institutionSummaryRepository
            .GetAll()
            .Where(w => w.RecordStatus == RecordStatus.Active
                && w.ReconciliationDate >= request.ReconciliationStartDate
                && w.ReconciliationDate <= request.ReconciliationEndDate
            )
            .ToListAsync();

        foreach (var summary in response.Items)
        {
            if (summary.ReconciliationStatus == ReconciliationStatus.Fail)
            {
                continue;
            }

            var hasFailedInstitution = institutionSummaries.Any(a => 
                a.ReconciliationStatus == ReconciliationStatus.Fail
                && a.ReconciliationDate == summary.ReconciliationDate
                && a.VendorId == summary.VendorId
            );

            if (hasFailedInstitution)
            {
                summary.ReconciliationStatus = ReconciliationStatus.Fail;
            }
        }

        return response;
    }

    public async Task CancelInstitutionPaymentByTransactionIdAsync(Guid transactionId)
    {
        var payment = await _institutionDetailRepository
            .GetAll()
            .SingleOrDefaultAsync(w => w.TransactionId == transactionId
                && w.RecordStatus == RecordStatus.Active
            );

        if (payment == null)
        {
            throw new NotFoundException($"ActiveInstitutionPaymentDetailNotFoundForTransactionId: {transactionId}");
        }

        payment.PaymentStatus = PaymentStatus.CancelledTransaction;

        await _institutionDetailRepository.UpdateAsync(payment);
    }
}
