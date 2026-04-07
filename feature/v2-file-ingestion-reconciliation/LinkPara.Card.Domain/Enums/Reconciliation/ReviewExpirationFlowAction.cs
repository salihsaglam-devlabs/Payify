using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;

public enum ReviewExpirationFlowAction : byte
{
    [Description("Continue with the remaining default flow.")]
    Continue = 0,

    [Description("Cancel the remaining flow.")]
    CancelRemaining = 1
}
