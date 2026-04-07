namespace LinkPara.Card.Application.Commons.Models.Scheduler;

public enum FileIngestionAndReconciliationJobType
{
    ImportCardTransactionsFromFtp = 1,
    ImportClearingFromFtp = 2,
    ExecutePendingOperations = 3,
    RegenerateOperations = 4
}
