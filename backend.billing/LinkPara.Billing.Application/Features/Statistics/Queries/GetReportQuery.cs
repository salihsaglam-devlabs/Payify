using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Application.Features.Statistics.Queries;

public class GetReportQuery : IRequest<ReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto>
{
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<InstitutionDetail> _reconciliationRepository;

    public GetReportQueryHandler(IGenericRepository<Transaction> transactionRepository,
        IGenericRepository<InstitutionDetail> reconciliationRepository)
    {
        _transactionRepository = transactionRepository;
        _reconciliationRepository = reconciliationRepository;
    }

    public async Task<ReportDto> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var reportDto = new ReportDto();

        var transactions = await _transactionRepository.GetAll()
            .Where(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.RecordStatus == RecordStatus.Active)
            .GroupBy(g => new { g.TransactionStatus })
            .Select(s => new
            {
                Status = s.Key.TransactionStatus,
                Count = s.Count(),
                Amount = s.Sum(k => k.BillAmount)
            })
            .ToListAsync(cancellationToken);

        if (transactions.Any())
        {
            var succeeded = transactions.Find(s => s.Status == TransactionStatus.Paid);
            reportDto.Succeeded = succeeded?.Count ?? 0;
            reportDto.SucceededAmount = succeeded?.Amount ?? 0;

            var notSucceeded = transactions.Where(s => s.Status == TransactionStatus.Error || s.Status == TransactionStatus.ProvisionError);
            foreach (var failed in notSucceeded)
            {
                reportDto.Failed += failed.Count;
                reportDto.FailedAmount += failed.Amount;
            }
        }

        reportDto.PendingReconciliation = await _reconciliationRepository.GetAll()
            .CountAsync(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.ReconciliationStatus == ReconciliationStatus.Fail, cancellationToken);

        return reportDto;
    }
}