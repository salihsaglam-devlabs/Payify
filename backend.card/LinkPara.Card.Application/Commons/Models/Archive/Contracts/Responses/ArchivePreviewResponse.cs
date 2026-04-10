using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;

public class ArchivePreviewResponse
{
    [MaxLength(2000)]
    public string? Message { get; set; }

    [Required]
    public List<ArchiveCandidateResult> Candidates { get; set; } = new();

    public List<ArchiveErrorDetail> Errors { get; set; } = new();

    public int ErrorCount { get; set; }
}
