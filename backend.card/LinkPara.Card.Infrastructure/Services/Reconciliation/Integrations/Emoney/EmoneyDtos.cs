using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

public sealed class EmoneyCustomerTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public string TransactionDirection { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string TagTitle { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string ExternalReferenceId { get; set; } = string.Empty;
    public Guid? RelatedTransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public Guid? CardTopupRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderAccountNumber { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string LastModifiedBy { get; set; } = string.Empty;
    public RecordStatus RecordStatus { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public bool IsReturned { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }
    public string CustomerTransactionId { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public bool IsSettlementReceived { get; set; }
    public decimal UsedCreditAmount { get; set; }
}

public sealed class EmoneyTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public string TransactionDirection { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public EmoneyCurrencyDto? Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PreBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string TagTitle { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string ExternalReferenceId { get; set; } = string.Empty;
    public Guid? RelatedTransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? IncomingTransactionId { get; set; }
    public Guid? WithdrawRequestId { get; set; }
    public Guid? CardTopupRequestId { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderAccountNumber { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string LastModifiedBy { get; set; } = string.Empty;
    public RecordStatus RecordStatus { get; set; }
    public EmoneyWalletDto? Wallet { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public string WalletName { get; set; } = string.Empty;
    public string CounterWalletNumber { get; set; } = string.Empty;
    public string CounterWalletName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string TotalAmountText { get; set; } = string.Empty;
    public bool IsReturned { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }
    public string CustomerTransactionId { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public bool IsSettlementReceived { get; set; }
    public decimal UsedCreditAmount { get; set; }
}

public sealed class EmoneyCurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string CurrencyType { get; set; } = string.Empty;
}

public sealed class EmoneyWalletDto
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string CurrencySymbol { get; set; } = string.Empty;
    public bool IsMainWallet { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool P2PCreditBalanceUsable { get; set; }
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }
    public decimal BlockedBalance { get; set; }
    public decimal BlockedBalanceCredit { get; set; }
    public decimal AvailableBalanceCredit { get; set; }
    public decimal AvailableBalanceCash { get; set; }
    public decimal AvailableBalance { get; set; }
}

public sealed class EmoneyCommandResult
{
    public bool IsSuccessful { get; init; }
    public string? ResponseBody { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static EmoneyCommandResult Success(string? responseBody)
        => new() { IsSuccessful = true, ResponseBody = responseBody };

    public static EmoneyCommandResult Failed(string errorCode, string errorMessage, string? responseBody = null)
        => new()
        {
            IsSuccessful = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ResponseBody = responseBody ?? errorMessage
        };
}
