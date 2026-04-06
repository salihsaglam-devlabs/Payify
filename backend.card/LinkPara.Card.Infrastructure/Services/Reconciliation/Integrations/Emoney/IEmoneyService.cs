namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

public interface IEmoneyService
{
    Task<IReadOnlyCollection<EmoneyCustomerTransactionDto>> GetByCustomerTransactionIdAsync(
        string customerTransactionId,
        CancellationToken cancellationToken = default);

    Task<EmoneyTransactionDto?> GetByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);
}
