using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum ReviewExpirationAction : byte
{
    [Description("Cancel the review.")]
    Cancel = 0,

    [Description("Approve the review.")]
    Approve = 1,

    [Description("Reject the review.")]
    Reject = 2
}
