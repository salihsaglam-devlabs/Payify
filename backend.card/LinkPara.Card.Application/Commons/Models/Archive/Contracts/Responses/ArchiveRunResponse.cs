using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveRunResponse
{
    [MaxLength(2000)]
    public string? Message { get; set; }

    [Range(0, int.MaxValue)]
    public int ProcessedCount { get; set; }

    [Range(0, int.MaxValue)]
    public int ArchivedCount { get; set; }

    [Range(0, int.MaxValue)]
    public int SkippedCount { get; set; }

    [Range(0, int.MaxValue)]
    public int FailedCount { get; set; }

    [Required]
    public List<ArchiveRunItemResult> Items { get; set; } = new();

    public List<ArchiveErrorDetail> Errors { get; set; } = new();

    public int ErrorCount { get; set; }
}
