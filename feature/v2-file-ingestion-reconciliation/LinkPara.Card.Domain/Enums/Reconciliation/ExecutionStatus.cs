
using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum ExecutionStatus : byte
{
    [Description("Execution started.")]
    Started = 0,

    [Description("Execution completed.")]
    Completed = 1,

    [Description("Execution failed.")]
    Failed = 2,

    [Description("Execution was skipped.")]
    Skipped = 3
}
