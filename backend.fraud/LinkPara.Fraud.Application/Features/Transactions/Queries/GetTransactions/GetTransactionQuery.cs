using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionQuery : IRequest<TransactionApiResponse>
{
    public string TransactionId { get; set; }
}

public class TransactionExistsQueryHandler : IRequestHandler<GetTransactionQuery, TransactionApiResponse>
{
    private readonly ITransactionService _transactionService;

    public TransactionExistsQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public async Task<TransactionApiResponse> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _transactionService.GetTransactionAsync(request.TransactionId);
    }
}
