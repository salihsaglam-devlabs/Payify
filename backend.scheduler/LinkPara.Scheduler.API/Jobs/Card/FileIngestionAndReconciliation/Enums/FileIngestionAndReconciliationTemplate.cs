namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

public enum FileIngestionAndReconciliationTemplate
{
    IngestRemoteCardBkm,
    IngestRemoteCardMsc,
    IngestRemoteCardVisa,

    IngestRemoteClearingBkm,
    IngestRemoteClearingMsc,
    IngestRemoteClearingVisa,

    IngestLocalCardBkm,
    IngestLocalCardMsc,
    IngestLocalCardVisa,

    IngestLocalClearingBkm,
    IngestLocalClearingMsc,
    IngestLocalClearingVisa,

    EvaluateDefault,
    ExecuteDefault
}