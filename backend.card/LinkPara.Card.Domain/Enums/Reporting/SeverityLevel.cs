using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum SeverityLevel
{
    [Description("Low severity - minor or no issues.")]
    LOW = 1,

    [Description("Medium severity - some failed lines.")]
    MEDIUM = 2,

    [Description("High severity - failure rate >= 20%.")]
    HIGH = 3,

    [Description("Critical severity - file completely failed.")]
    CRITICAL = 4
}

