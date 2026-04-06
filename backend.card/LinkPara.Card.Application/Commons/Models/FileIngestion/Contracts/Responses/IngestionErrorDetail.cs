namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;

public class IngestionErrorDetail
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public string? Step { get; set; }

    public long? LineNumber { get; set; }

    public string? FileName { get; set; }

    public string? FieldName { get; set; }

    public string? RecordType { get; set; }

    public string Severity { get; set; } = "Error";
}