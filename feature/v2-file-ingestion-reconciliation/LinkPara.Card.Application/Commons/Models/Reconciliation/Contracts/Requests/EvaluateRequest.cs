using System;
namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class EvaluateRequest
{
    public Guid[] IngestionFileIds { get; set; } = Array.Empty<Guid>();

    public EvaluateOptions? Options { get; set; }
}
