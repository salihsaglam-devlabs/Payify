using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Statistics.Queries;

public class GetReportQuery : IRequest<ReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto>
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<TransferOrder> _transferOrderRepository;

    public GetReportQueryHandler(IGenericRepository<Wallet> walletRepository,
        IGenericRepository<Transaction> transactionRepository,
        IGenericRepository<TransferOrder> transferOrderRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _transferOrderRepository = transferOrderRepository;
    }

    public async Task<ReportDto> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var activeWallet = _walletRepository.GetAll()
            .Count(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate);

        var passiveWallet = _walletRepository.GetAll()
            .Count(s =>
                s.RecordStatus == RecordStatus.Passive &&
                s.UpdateDate >= request.StartDate && s.UpdateDate <= request.EndDate);

        var report = new ReportDto
        {
            ActiveWallet = activeWallet,
            PassiveWallet = passiveWallet,
            TotalWallet = activeWallet + passiveWallet
        };

        var transactions = await _transactionRepository.GetAll()
            .Where(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.RecordStatus == RecordStatus.Active)
            .GroupBy(g => new { g.TransactionStatus })
            .Select(s => new
            {
                Status = s.Key.TransactionStatus,
                Count = s.Count(),
                Amount = s.Sum(k => k.Amount),
            })
            .ToListAsync(cancellationToken);

        if (transactions is not null)
        {
            var failed = transactions.FirstOrDefault(s => s.Status == TransactionStatus.Failed);
            report.FailedTransactionAmount = failed?.Amount ?? 0;
            report.FailedTransaction = failed?.Count ?? 0;

            var completed = transactions.FirstOrDefault(s => s.Status == TransactionStatus.Completed);
            report.SucceededTransactionAmount = completed?.Amount ?? 0;
            report.SucceededTransaction = completed?.Count ?? 0;
        }

        var failedFutureDateTransferOrder = _transferOrderRepository.GetAll()
            .Where(s => s.RecordStatus == RecordStatus.Active && 
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.TransferOrderStatus == TransferOrderStatus.Failed);

        report.FailedFutureDateTransferAmount = failedFutureDateTransferOrder.Sum(s => s.Amount);
        report.FailedFutureDateTransferOrder = failedFutureDateTransferOrder.Count();

        // todo : donus bekleniyor
        report.CancelledTransaction = 0;
        report.CancelledTransactionAmount = 0;
        report.PendingApprovalTransaction = 0;

        return report;
    }
}