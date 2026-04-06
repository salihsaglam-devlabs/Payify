using System.ComponentModel;

namespace LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation;

public enum FileIngestionAndReconciliationJobType
{
    [Description("Job to ingest and process the input file.")]
    IngestFile = 1,

    [Description("Job to evaluate reconciliation rules.")]
    Evaluate = 2,

    [Description("Job to execute reconciliation actions.")]
    Execute = 3
}