using System.ComponentModel;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

public enum JobRequestType
{
    [Description("Ingest file request.")]
    IngestFile = 1,

    [Description("Evaluate request.")]
    Evaluate = 2,

    [Description("Execute request.")]
    Execute = 3
}