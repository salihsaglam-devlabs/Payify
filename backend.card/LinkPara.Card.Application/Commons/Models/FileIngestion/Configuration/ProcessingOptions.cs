namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ProcessingOptions
{
    public int? BatchSize { get; set; }
    public int? RetryBatchSize { get; set; }
    public int? FailedRowMaxRetryCount { get; set; }
    public bool? UseBulkInsert { get; set; }
    public bool? EnableParallelProcessing { get; set; }
    public int? MaxDegreeOfParallelism { get; set; }

    public void Validate()
    {
        if (BatchSize is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.BatchSize");
        if (RetryBatchSize is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.RetryBatchSize");
        if (FailedRowMaxRetryCount is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.FailedRowMaxRetryCount");
        if (UseBulkInsert is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.UseBulkInsert");
        if (EnableParallelProcessing is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.EnableParallelProcessing");
        if (MaxDegreeOfParallelism is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing.MaxDegreeOfParallelism");
        if (BatchSize <= 0)
            throw new InvalidOperationException($"FileIngestion.Processing.BatchSize must be positive. Current: {BatchSize}");
        if (RetryBatchSize <= 0)
            throw new InvalidOperationException($"FileIngestion.Processing.RetryBatchSize must be positive. Current: {RetryBatchSize}");
        if (MaxDegreeOfParallelism <= 0)
            throw new InvalidOperationException($"FileIngestion.Processing.MaxDegreeOfParallelism must be positive. Current: {MaxDegreeOfParallelism}");
    }
}
