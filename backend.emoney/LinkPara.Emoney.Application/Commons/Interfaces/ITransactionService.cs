using LinkPara.Emoney.Application.Features.Transactions;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto>  GetTransactionWithDetailsAsync(Guid transactionId, CancellationToken cancellationToken);

    Task<List<TransactionDto>> GetWalletTransactionsWithDetailsAsync(List<Guid> transactionIds, CancellationToken cancellationToken);
}