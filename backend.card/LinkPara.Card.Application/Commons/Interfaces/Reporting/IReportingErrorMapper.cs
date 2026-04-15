#nullable enable
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IReportingErrorMapper
{
    ReconciliationErrorDetail MapException(
        Exception ex,
        string step,
        string? detail = null,
        string? message = null);

    ReconciliationErrorDetail Create(
        string code,
        string message,
        string step,
        string? detail = null,
        string severity = "Error");
}

