using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum ArchiveEligibilityStatus
{
    [Description("File is eligible for archiving.")]
    ELIGIBLE = 1,

    [Description("File has already been archived.")]
    ALREADY_ARCHIVED = 2,

    [Description("File processing is not yet complete.")]
    FILE_NOT_COMPLETE = 3,

    [Description("Reconciliation is still pending for some lines.")]
    RECON_PENDING = 4
}

