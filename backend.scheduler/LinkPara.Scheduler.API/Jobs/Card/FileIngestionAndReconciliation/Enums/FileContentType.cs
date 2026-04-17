using System.ComponentModel;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

public enum FileContentType
{
    [Description("BKM format.")]
    Bkm = 1,

    [Description("Mastercard format.")]
    Msc = 2,

    [Description("Visa format.")]
    Visa = 3
}