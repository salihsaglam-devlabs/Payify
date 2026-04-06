
using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum OperationStatus : byte
{
    [Description("Operation is planned.")]
    Planned = 0,

    [Description("Operation is blocked by a previous operation or manual decision.")]
    Blocked = 1,

    [Description("Operation is executing.")]
    Executing = 2,

    [Description("Operation completed successfully.")]
    Completed = 3,

    [Description("Operation failed.")]
    Failed = 4,

    [Description("Operation was cancelled.")]
    Cancelled = 5
}
