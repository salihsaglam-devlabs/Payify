using System.ComponentModel;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

public enum FileType
{
    [Description("Card transaction file.")]
    Card = 1,

    [Description("Clearing/reconciliation file.")]
    Clearing = 2
}