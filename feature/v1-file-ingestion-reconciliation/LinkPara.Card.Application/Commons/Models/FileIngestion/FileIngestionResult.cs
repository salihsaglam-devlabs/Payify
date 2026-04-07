namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionResult
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public int TotalLines { get; set; }
    public int ParsedRecords { get; set; }
    public int ReconciledCards { get; set; }
    public int ReconciliationOperations { get; set; }
    public int ReconciliationManualReviewItems { get; set; }
    public int PlannedAutoOperations { get; set; }
    public bool AutoExecutionTriggered { get; set; }
    public int PendingAutoOperations { get; set; }
    public bool HasErrors { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}
