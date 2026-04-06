using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Statistics.Queries;

public class GetReportQuery : IRequest<ReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto>
{
    private readonly IGenericRepository<MerchantPool> _merchantPoolRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IGenericRepository<BankHealthCheck> _bankHealthCheckRepository;
    private readonly IGenericRepository<PostingTransferError> _postingTransferError;


    public GetReportQueryHandler(
        IGenericRepository<MerchantPool> merchantPoolRepository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IGenericRepository<PostingTransferError> postingTransferError,
        IGenericRepository<BankHealthCheck> bankHealthCheckRepository)
    {
        _merchantPoolRepository = merchantPoolRepository;
        _merchantRepository = merchantRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _postingTransferError = postingTransferError;
        _bankHealthCheckRepository = bankHealthCheckRepository;
    }

    public async Task<ReportDto> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var report = new ReportDto();

        await FillApplicationInformationAsync(request, report);

        await FillMerchantInformationAsync(request, report);

        await FillActionInformationAsync(request, report);

        await FillTransactionInformationAsync(request, report);

        return report;
    }

    public async Task<ReportDto> FillMerchantInformationAsync(GetReportQuery request, ReportDto report)
    {
        var allMerchantsQuery = _merchantRepository.GetAll()
            .Where(s => s.RecordStatus == RecordStatus.Active);

        var totalMerchantInfo = await allMerchantsQuery.GroupBy(g => new { g.MerchantStatus })
            .Select(s => new
            {
                Status = s.Key.MerchantStatus,
                Count = s.Count()
            }).ToListAsync();

        var filteredMerchantInfo = await allMerchantsQuery.Where(s => s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate)
            .GroupBy(g => new { g.MerchantStatus })
            .Select(s => new
            {
                Status = s.Key.MerchantStatus,
                Count = s.Count()
            }).ToListAsync();


        report.ActiveMerchant = filteredMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Active)?.Count ?? 0;
        report.PendingRiskApprovalApplication = filteredMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.RiskApproval)?.Count ?? 0;
        report.InProgressApplication = filteredMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Draft || s.Status == MerchantStatus.MissingDocument)?.Count ?? 0;
        report.ClosedMerchant = filteredMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Closed)?.Count ?? 0;
        report.RejectedMerchant = filteredMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Reject)?.Count ?? 0;
        report.TotalActiveMerchant = totalMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Active)?.Count ?? 0;
        report.TotalClosedMerchant= totalMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Closed)?.Count ?? 0;
        report.TotalRejectedMerchant = totalMerchantInfo.FirstOrDefault(s => s.Status == MerchantStatus.Reject)?.Count ?? 0;

        return report;
    }

    public async Task<ReportDto> FillApplicationInformationAsync(GetReportQuery request, ReportDto report)
    {
        var pendingApplication = await _merchantPoolRepository.GetAll()
            .CountAsync(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.MerchantPoolStatus == MerchantPoolStatus.Waiting &&
                s.RecordStatus == RecordStatus.Active);

        report.PendingApplication = pendingApplication;

        var bankHealthCheck = String.Join(", ", await _bankHealthCheckRepository
            .GetAll().Include(b => b.AcquireBank).ThenInclude(b => b.Bank)
            .Where(b => b.HealthCheckType == HealthCheckType.Unhealthy)
            .Select(a => a.AcquireBank.Bank.Name).ToArrayAsync());

        report.UnhealtyBankNames = bankHealthCheck;

        return report;
    }

    public async Task<ReportDto> FillActionInformationAsync(GetReportQuery request, ReportDto report)
    {
        var actionRequiredQuery =  _timeoutTransactionRepository.GetAll();

        var actionRequired = await actionRequiredQuery.CountAsync(s =>
                s.UpdateDate >= request.StartDate && s.UpdateDate <= request.EndDate &&
                s.TimeoutTransactionStatus == TimeoutTransactionStatus.CancelFail &&
                s.RecordStatus == RecordStatus.Active);

        var totalTimeoutProcessCount = await actionRequiredQuery.CountAsync(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate);

        report.ActionRequired = actionRequired;
        report.TotalTimeoutProcessCount = totalTimeoutProcessCount;

        var postingTransferError = await _postingTransferError
            .GetAll()
            .CountAsync(s => s.PostingDate >= request.StartDate
                          && s.PostingDate <= request.EndDate
                          && s.RecordStatus == RecordStatus.Active);

        report.NoEndOfDayTransaction = postingTransferError;

        return report;
    }

    public async Task<ReportDto> FillTransactionInformationAsync(GetReportQuery request, ReportDto report)
    {
        var transactions = await _merchantTransactionRepository.GetAll()
            .Where(s => s.RecordStatus == RecordStatus.Active)
            .GroupBy(s => new
            {
                s.TransactionStatus,
                s.TransactionType,
                s.IsChargeback,
                s.IsSuspecious,
                s.IsManualReturn,
                CreateDate = s.CreateDate.Date,
                ReverseDate = s.ReverseDate.Date,
            })
            .Select(s => new
            {
                s.Key.TransactionStatus,
                s.Key.TransactionType,
                s.Key.IsChargeback,
                s.Key.IsSuspecious,
                s.Key.CreateDate,
                s.Key.ReverseDate,
                s.Key.IsManualReturn,
                TotalCount = s.Count(),
                TotalAmount = s.Sum(s => s.Amount)
            }).ToListAsync();

        var auths = transactions.Where(s =>
            s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate);

        foreach (var item in auths)
        {
            if ((item.TransactionType == TransactionType.Auth || item.TransactionType == TransactionType.PostAuth) &&
                item.TransactionStatus == TransactionStatus.Success)
            {
                report.SucceededProvision += item.TotalCount;
                report.SucceededProvisionAmount += item.TotalAmount;
            }

            if ((item.TransactionType == TransactionType.Auth || item.TransactionType == TransactionType.PostAuth) &&
                item.TransactionStatus == TransactionStatus.Fail)
            {
                report.FailedProvision += item.TotalCount;
                report.FailedProvisionAmount += item.TotalAmount;
            }

            if ((item.TransactionType == TransactionType.Return) &&
                item.TransactionStatus == TransactionStatus.Success)
            {
                report.SucceededReturn += item.TotalCount;
                report.SucceededReturnAmount += item.TotalAmount;
            }

            if ((item.TransactionType == TransactionType.Return) &&
             item.TransactionStatus == TransactionStatus.Fail)
            {
                report.FailedReturn += item.TotalCount;
                report.FailedReturnAmount += item.TotalAmount;
            }

            if (item.IsManualReturn)
            {
                report.ManualReturnCount += item.TotalCount;
            }
        }

        var succeededReverses = transactions.Where(s =>
            s.ReverseDate > DateTime.MinValue &&
            s.ReverseDate >= request.StartDate && s.ReverseDate <= request.EndDate &&
            s.TransactionStatus == TransactionStatus.Reversed);

        report.SucceededCancel = succeededReverses.Sum(s => s.TotalCount);
        report.SucceededCancelAmount = succeededReverses.Sum(s => s.TotalAmount);

        var failedReverses = _bankTransactionRepository.GetAll()
            .Where(s => s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
            s.TransactionType == TransactionType.Reverse &&
            s.TransactionStatus == TransactionStatus.Fail);

        report.FailedCancel = await failedReverses.CountAsync();
        report.FailedCancelAmount = await failedReverses.SumAsync(s => s.Amount);

        var updatedTransactions = await _merchantTransactionRepository.GetAll()
                    .Where(s => s.UpdateDate.HasValue && s.UpdateDate.Value >= request.StartDate && s.UpdateDate.Value <= request.EndDate)
                    .GroupBy(s => new { s.IsSuspecious, s.IsChargeback })
                    .Select(s => new
                    {
                        s.Key.IsSuspecious,
                        s.Key.IsChargeback,
                        TotalCount = s.Count()
                    }).ToListAsync();

        report.SuspeciousPayment = updatedTransactions.FirstOrDefault(s => s.IsSuspecious)?.TotalCount ?? 0;
        report.Chargeback = updatedTransactions.FirstOrDefault(s => s.IsChargeback)?.TotalCount ?? 0;

        return report;
    }
}