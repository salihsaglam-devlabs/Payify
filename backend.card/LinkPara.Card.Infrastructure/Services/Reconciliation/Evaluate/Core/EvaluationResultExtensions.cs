using LinkPara.Card.Domain.Enums.Reconciliation;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal static class EvaluationResultExtensions
{
    public static EvaluationResult SetNote(this EvaluationResult result, string note)
    {
        result.Note = note;
        return result;
    }

    public static void AddAutoOperation(
        this EvaluationResult result,
        string code,
        string note,
        Dictionary<string, List<OperationPayloadItem>> payload)
    {
        result.Operations.Add(new EvaluationOperation
        {
            Code = code,
            Note = note,
            Branch = Branches.Default,
            IsManual = false,
            Order = result.Operations.Count,
            Payload = payload
        });
    }

    public static void AddManualOperation(
        this EvaluationResult result,
        string gateCode,
        string gateNote,
        Func<string, Dictionary<string, List<OperationPayloadItem>>> payloadFactory,
        string approveCode,
        string approveNote,
        string rejectCode,
        string rejectNote,
        TimeSpan? reviewTimeout = null,
        ReviewExpirationAction expirationAction = ReviewExpirationAction.Cancel,
        ReviewExpirationFlowAction expirationFlowAction = ReviewExpirationFlowAction.Continue)
    {
        result.Operations.Add(new EvaluationOperation
        {
            Code = gateCode,
            Note = gateNote,
            Branch = Branches.Default,
            IsManual = true,
            Order = result.Operations.Count,
            Payload = payloadFactory(gateCode),
            ReviewTimeout = reviewTimeout ?? TimeSpan.FromDays(1),
            ExpirationAction = expirationAction,
            ExpirationFlowAction = expirationFlowAction
        });

        result.Operations.Add(new EvaluationOperation
        {
            Code = approveCode,
            Note = approveNote,
            Branch = Branches.Approve,
            IsManual = false,
            Order = result.Operations.Count,
            Payload = payloadFactory(approveCode)
        });

        result.Operations.Add(new EvaluationOperation
        {
            Code = rejectCode,
            Note = rejectNote,
            Branch = Branches.Reject,
            IsManual = false,
            Order = result.Operations.Count,
            Payload = payloadFactory(rejectCode)
        });
    }
}
