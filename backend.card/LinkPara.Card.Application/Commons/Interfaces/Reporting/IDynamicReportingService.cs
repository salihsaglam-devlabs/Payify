using LinkPara.Card.Application.Features.Reporting.Queries.Dynamic;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IDynamicReportingService
{
    Task<DynamicReportingResponse> ExecuteAsync(GetDynamicReportingQuery query, CancellationToken ct);
}

