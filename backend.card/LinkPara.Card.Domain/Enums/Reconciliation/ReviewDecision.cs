
using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum ReviewDecision : byte
{
    [Description("Review is pending.")]
    Pending = 0,

    [Description("Review approved.")]
    Approved = 1,

    [Description("Review rejected.")]
    Rejected = 2,

    [Description("Review was cancelled.")]
    Cancelled = 3
}
