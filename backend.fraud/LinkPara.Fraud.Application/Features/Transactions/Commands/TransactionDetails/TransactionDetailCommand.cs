using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.TransactionDetails;

public class TransactionDetailCommand : IRequest<TransactionDetailResponse>
{
    public string TransactionId { get; set; }
}

public class TransactionDetailCommandHandler : IRequestHandler<TransactionDetailCommand, TransactionDetailResponse>
{
    private readonly ITransactionService _transactionService;

    public TransactionDetailCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public async Task<TransactionDetailResponse> Handle(TransactionDetailCommand request, CancellationToken cancellationToken)
    {
        return await _transactionService.GetTransactionDetailAsync(request.TransactionId);
    }
}
