using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.CancelTransactions;

public class CancelTransactionCommand : IRequest<CancelTransactionResponse>
{
    public string TransactionId { get; set; }
}
public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand, CancelTransactionResponse>
{
    private readonly ITransactionService _transactionService;

    public CancelTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public async Task<CancelTransactionResponse> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _transactionService.CancelTransactionAsync(request.TransactionId);
    }
}