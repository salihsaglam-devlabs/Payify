using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public abstract class ReconciliationResponseBase
{
    [Range(0, int.MaxValue)]
    public int ErrorCount { get; set; }

    [Required]
    public List<ReconciliationErrorDetail> Errors { get; set; } = new();
}
