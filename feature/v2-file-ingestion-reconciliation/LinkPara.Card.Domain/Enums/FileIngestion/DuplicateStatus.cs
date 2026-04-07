using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum DuplicateStatus
{
    [Description("Record is unique, no duplicates found.")]
    Unique = 1,

    [Description("Primary record among duplicates.")]
    Primary = 2,

    [Description("Secondary record among duplicates.")]
    Secondary = 3,

    [Description("Conflicting duplicate records.")]
    Conflict = 4
}