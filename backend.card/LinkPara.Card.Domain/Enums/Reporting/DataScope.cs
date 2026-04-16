using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum DataScope
{
    [Description("Live/active data.")]
    LIVE = 1,

    [Description("Archived data.")]
    ARCHIVE = 2
}

