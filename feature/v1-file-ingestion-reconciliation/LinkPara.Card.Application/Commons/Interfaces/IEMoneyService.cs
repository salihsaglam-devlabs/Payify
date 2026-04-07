using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IEMoneyService
{
    Task<EMoneyTransactionLookupResult> GetByCustomerTransactionIdAsync(
        string customerTransactionId,
        CancellationToken cancellationToken = default);

    Task<bool> SetExpireStatusByCustomerTransactionIdAsync(
        string customerTransactionId,
        string operationIdempotencyKey = null,
        CancellationToken cancellationToken = default);

    Task<bool> CreateTransactionForCardReconciliationAsync(
        string customerTransactionId,
        string referenceCustomerTransactionId,
        decimal? amount,
        string currencyCode,
        string operationIdempotencyKey = null,
        CancellationToken cancellationToken = default);
}
