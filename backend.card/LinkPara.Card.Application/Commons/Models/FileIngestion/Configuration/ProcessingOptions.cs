using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class ProcessingOptions
{
    public const int DefaultBatchSize = 50_000;
    public const int DefaultRetryBatchSize = 10_000;
    public const int DefaultFailedRowMaxRetryCount = 3;
    public const bool DefaultUseBulkInsert = true;
    public const bool DefaultEnableParallelProcessing = true;
    public const int DefaultMaxDegreeOfParallelism = 8;

    public int? BatchSize { get; set; }
    public int? RetryBatchSize { get; set; }
    public int? FailedRowMaxRetryCount { get; set; }
    public bool? UseBulkInsert { get; set; }
    public bool? EnableParallelProcessing { get; set; }
    public int? MaxDegreeOfParallelism { get; set; }

    public void ValidateAndApplyDefaults()
    {
        BatchSize ??= DefaultBatchSize;
        RetryBatchSize ??= DefaultRetryBatchSize;
        FailedRowMaxRetryCount ??= DefaultFailedRowMaxRetryCount;
        UseBulkInsert ??= DefaultUseBulkInsert;
        EnableParallelProcessing ??= DefaultEnableParallelProcessing;
        MaxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;

        if (BatchSize <= 0)
            throw new FileIngestionProcessingBatchSizeInvalidException($"FileIngestion.Processing.BatchSize must be positive. Current: {BatchSize}");
        if (RetryBatchSize <= 0)
            throw new FileIngestionProcessingRetryBatchSizeInvalidException($"FileIngestion.Processing.RetryBatchSize must be positive. Current: {RetryBatchSize}");
        if (MaxDegreeOfParallelism <= 0)
            throw new FileIngestionProcessingMaxParallelismInvalidException($"FileIngestion.Processing.MaxDegreeOfParallelism must be positive. Current: {MaxDegreeOfParallelism}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
