using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum ReportingSortBy
{
    [Description("Sort by card transaction date.")]
    CardTransactionDate = 1,

    [Description("Sort by card record creation date.")]
    CardCreateDate = 2,

    [Description("Sort by network.")]
    Network = 3,

    [Description("Sort by match status.")]
    MatchStatus = 4,

    [Description("Sort by amount difference.")]
    AmountDifference = 5
}

