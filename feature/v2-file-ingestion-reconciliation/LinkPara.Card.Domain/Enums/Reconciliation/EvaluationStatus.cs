
using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum EvaluationStatus : byte
{
    [Description("Evaluation is pending.")]
    Pending = 0,

    [Description("Evaluation in progress.")]
    Evaluating = 1,

    [Description("Evaluation has been planned.")]
    Planned = 2,

    [Description("Evaluation failed.")]
    Failed = 3,

    [Description("Evaluation completed successfully.")]
    Completed = 4
}
