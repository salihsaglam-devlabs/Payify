namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ProcessingOptions
{
    public int BatchSize { get; set; } = 5000;
    public int RetryBatchSize { get; set; } = 1000;
    public int FailedRowMaxRetryCount { get; set; } = 3;
    public bool UseBulkInsert { get; set; } = true;
    public bool EnableParallelProcessing { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 5;
}
