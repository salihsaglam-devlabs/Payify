using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public abstract class ReconciliationResponseBase
{
    [MaxLength(1000)]
    public string? Message { get; set; }

    [Range(0, int.MaxValue)]
    public int ErrorCount { get; set; }

    [Required]
    public List<ReconciliationErrorDetail> Errors { get; set; } = new();
}
