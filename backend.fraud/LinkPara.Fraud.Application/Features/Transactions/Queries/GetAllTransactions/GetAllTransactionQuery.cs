using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllTransactions;

public class GetAllTransactionQuery : SearchQueryParams, IRequest<PaginatedList<TransactionMonitoringDto>> 
{
    public string CommandName { get; set; }
    public string Module { get; set; }
    public string SenderNumber { get; set; }
    public string ReceiverNumber { get; set; }
    public string SenderName { get; set; }
    public string ReceiverName { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public MonitoringStatus? MonitoringStatus { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
}

public class GetAllTransactionQueryHandler : IRequestHandler<GetAllTransactionQuery, PaginatedList<TransactionMonitoringDto>>
{
    private readonly ITransactionService _transactionService;

    public GetAllTransactionQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public async Task<PaginatedList<TransactionMonitoringDto>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _transactionService.GetListAsync(request);
    }
}
