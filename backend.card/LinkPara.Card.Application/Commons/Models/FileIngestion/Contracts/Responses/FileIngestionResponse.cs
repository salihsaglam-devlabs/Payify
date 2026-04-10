using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;

public class FileIngestionResponse
{
    public Guid FileId { get; set; }
    public string FileKey { get; set; }
    public string FileName { get; set; }
    public FileStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string Message { get; set; }
    public long TotalCount { get; set; }
    public long SuccessCount { get; set; }
    public long ErrorCount { get; set; }
    public List<IngestionErrorDetail> Errors { get; set; } = new();
}