using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EvaluateResponse : ReconciliationResponseBase
{
    public Guid EvaluationRunId { get; set; }

    [Range(0, int.MaxValue)]
    public int CreatedOperationsCount { get; set; }

    [Range(0, int.MaxValue)]
    public int SkippedCount { get; set; }

}
