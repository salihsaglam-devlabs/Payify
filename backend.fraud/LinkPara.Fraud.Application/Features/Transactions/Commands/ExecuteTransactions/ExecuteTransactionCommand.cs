using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Transactions;
using LinkPara.HttpProviders.Fraud.Models;
using MediatR;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.ExecuteTransactions;

public class ExecuteTransactionCommand : IRequest<TransactionResponse>
{
    public FraudCheckRequest FraudCheckRequest { get; set; }
}
public class ExecuteTransactionCommandHandler : IRequestHandler<ExecuteTransactionCommand, TransactionResponse>
{
    private readonly ITransactionService _transactionService;

    public ExecuteTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public async Task<TransactionResponse> Handle(ExecuteTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _transactionService.ExecuteTransactionAsync(request.FraudCheckRequest);
    }
}

