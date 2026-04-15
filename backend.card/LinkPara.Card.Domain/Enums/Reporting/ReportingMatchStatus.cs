using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum ReportingMatchStatus
{
    [Description("Card transaction matched with clearing transaction.")]
    Matched = 1,

    [Description("Card transaction has no matching clearing record.")]
    UnmatchedCard = 2
}

