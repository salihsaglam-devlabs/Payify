using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

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
