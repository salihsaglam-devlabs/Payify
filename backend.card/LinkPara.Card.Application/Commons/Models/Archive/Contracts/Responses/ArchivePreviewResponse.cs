using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchivePreviewResponse
{
    [Required]
    public List<ArchiveCandidateResult> Candidates { get; set; } = new();
}
