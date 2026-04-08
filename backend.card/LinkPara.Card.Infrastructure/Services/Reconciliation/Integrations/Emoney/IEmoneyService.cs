namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

public interface IEmoneyService
{
    Task<IReadOnlyCollection<EmoneyCustomerTransactionDto>> GetByCustomerTransactionIdAsync(
        string customerTransactionId,
        CancellationToken cancellationToken = default);

    Task<EmoneyTransactionDto?> GetByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> UpdateTransactionStatusAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> ReverseBalanceEffectAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> CorrectResponseCodeAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> ExpireTransactionAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> CreateTransactionAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> RefundTransactionAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> InitChargebackAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> ApproveChargebackAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> CreateShadowBalanceDebtCreditAsync(
        object request,
        CancellationToken cancellationToken = default);

    Task<EmoneyCommandResult> RunShadowBalanceProcessAsync(
        object request,
        CancellationToken cancellationToken = default);
}
