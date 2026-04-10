#nullable enable
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Interfaces.Reconciliation;

public interface IReconciliationErrorMapper
{
    ReconciliationErrorDetail MapException(
        Exception ex,
        string step,
        Guid? fileLineId = null,
        Guid? operationId = null,
        Guid? evaluationId = null,
        string? detail = null,
        string? message = null);

    ReconciliationErrorDetail Create(
        string code,
        string message,
        string step,
        Guid? fileLineId = null,
        Guid? operationId = null,
        Guid? evaluationId = null,
        string? detail = null,
        string severity = "Error");
}
