using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ExecuteResponse : ReconciliationResponseBase
{
    [Range(0, int.MaxValue)]
    public int TotalAttempted { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalSucceeded { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalFailed { get; set; }

    [Required]
    public List<OperationExecutionResult> Results { get; set; } = new();
}
