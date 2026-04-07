using LinkPara.Card.Application.Commons.Constants;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionOperationResponse
{
    public bool IsSuccess { get; set; }
    public string SourceType { get; set; }
    public int TotalFiles { get; set; }
    public IReadOnlyCollection<FileIngestionResult> Files { get; set; } = [];
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }

    public static FileIngestionOperationResponse FromResults(string sourceType, IReadOnlyCollection<FileIngestionResult> results)
    {
        var files = results ?? [];
        var totalFiles = files.Count;
        var failedCount = files.Count(x =>
            string.Equals(x.Status, FileIngestionResultStatuses.Failed, StringComparison.OrdinalIgnoreCase) ||
            x.HasErrors);

        if (totalFiles == 0)
        {
            return new FileIngestionOperationResponse
            {
                IsSuccess = false,
                SourceType = sourceType,
                TotalFiles = 0,
                Files = files,
                ErrorCode = FileIngestionErrorCodes.NoFilesFound,
                ErrorMessage = "No files matched the ingestion criteria."
            };
        }

        return new FileIngestionOperationResponse
        {
            IsSuccess = failedCount == 0,
            SourceType = sourceType,
            TotalFiles = totalFiles,
            Files = files,
            ErrorCode = failedCount == 0 ? null : FileIngestionErrorCodes.PartialOrFullFailure,
            ErrorMessage = failedCount == 0 ? null : $"{failedCount} of {totalFiles} file(s) failed during ingestion."
        };
    }
}
