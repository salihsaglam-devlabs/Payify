using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

public class EvaluateResponse : ReconciliationResponseBase
{
    public Guid EvaluationRunId { get; set; }

    [Range(0, int.MaxValue)]
    public int CreatedOperationsCount { get; set; }

    [Range(0, int.MaxValue)]
    public int SkippedCount { get; set; }

}
