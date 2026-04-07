using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal abstract class EvaluationContext
{
    public required IngestionFileLine RootRow { get; init; }
    public required IngestionFile RootFile { get; init; }
    public required string CorrelationKey { get; init; }
    public required string CorrelationValue { get; init; }
}

internal sealed class BkmEvaluationContext : EvaluationContext
{
    public required IReadOnlyList<CardBkmDetail> CardDetails { get; init; }
    public required IReadOnlyList<ClearingBkmDetail> ClearingDetails { get; init; }
    public required IReadOnlyList<EmoneyCustomerTransactionDto> EmoneyTransactions { get; init; }
}

internal sealed class VisaEvaluationContext : EvaluationContext
{
    public required IReadOnlyList<CardVisaDetail> CardDetails { get; init; }
    public required IReadOnlyList<ClearingVisaDetail> ClearingDetails { get; init; }
    public required IReadOnlyList<EmoneyCustomerTransactionDto> EmoneyTransactions { get; init; }
}

internal sealed class MscEvaluationContext : EvaluationContext
{
    public required IReadOnlyList<CardMscDetail> CardDetails { get; init; }
    public required IReadOnlyList<ClearingMscDetail> ClearingDetails { get; init; }
    public required IReadOnlyList<EmoneyCustomerTransactionDto> EmoneyTransactions { get; init; }
}

internal sealed class EvaluationResult
{
    public string Note { get; set; } = string.Empty;
    public List<EvaluationOperation> Operations { get; set; } = [];
}

internal sealed class EvaluationOperation
{
    public string Code { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsManual { get; set; }
    public string Branch { get; set; } = Branches.Default;
    public int Order { get; set; }
    public Dictionary<string, List<OperationPayloadItem>> Payload { get; set; } = [];
    public TimeSpan? ReviewTimeout { get; set; }
    public ReviewExpirationAction? ExpirationAction { get; set; }
    public ReviewExpirationFlowAction? ExpirationFlowAction { get; set; }
}

internal sealed class OperationPayloadItem
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}

internal static class Branches
{
    public const string Default = "Default";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
}
