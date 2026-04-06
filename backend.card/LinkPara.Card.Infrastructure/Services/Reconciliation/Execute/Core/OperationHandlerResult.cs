namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute;

internal sealed class OperationHandlerResult
{
    public required string ResultCode { get; init; }
    public required string ResultMessage { get; init; }
    public required bool IsSuccessful { get; init; }
    public required bool IsSkipped { get; init; }
    public object? RequestPayload { get; init; }
    public object? ResponsePayload { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}
