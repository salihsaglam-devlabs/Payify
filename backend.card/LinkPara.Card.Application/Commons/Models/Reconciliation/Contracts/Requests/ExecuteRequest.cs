using System;
namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ExecuteRequest
{
    public Guid[] GroupIds { get; set; } = Array.Empty<Guid>();

    public Guid[] OperationIds { get; set; } = Array.Empty<Guid>();

    public ExecuteOptions? Options { get; set; }
}
