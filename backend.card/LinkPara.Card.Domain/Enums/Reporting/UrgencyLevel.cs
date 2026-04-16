using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum UrgencyLevel
{
    [Description("Normal urgency.")]
    NORMAL = 1,

    [Description("Review is overdue (waiting > 24h).")]
    OVERDUE = 2,

    [Description("Review expiration is approaching (< 4h).")]
    EXPIRING_SOON = 3,

    [Description("Review has already expired.")]
    EXPIRED = 4
}

