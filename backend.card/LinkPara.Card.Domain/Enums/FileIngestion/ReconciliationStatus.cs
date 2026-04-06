using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum ReconciliationStatus
{
    [Description("Reconciliation is ready to be processed.")]
    Ready = 1,

    [Description("Reconciliation process failed.")]
    Failed = 2,

    [Description("Reconciliation completed successfully.")]
    Success = 3,

    [Description("Reconciliation is currently being processed.")]
    Processing = 4
}
