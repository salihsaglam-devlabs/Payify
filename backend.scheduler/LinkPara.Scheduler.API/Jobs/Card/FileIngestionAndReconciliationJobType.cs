namespace LinkPara.Scheduler.API.Jobs.Card;

public enum FileIngestionAndReconciliationJobType
{
    ImportCardTransactionsFromFtp = 1,
    ImportClearingFromFtp = 2,
    ExecutePendingOperations = 3,
    RegenerateOperations = 4
}
