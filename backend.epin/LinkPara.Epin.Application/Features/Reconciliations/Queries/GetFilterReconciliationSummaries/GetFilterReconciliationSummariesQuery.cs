using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Features.Reconciliations.Queries.GetFilterReconciliationSummaries;

public class GetFilterReconciliationSummariesQuery : SearchQueryParams, IRequest<PaginatedList<ReconciliationSummaryDto>>
{
    public DateTime? ReconciliationDateStart { get; set; }
    public DateTime? ReconciliationDateEnd { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public Organization? Organization { get; set; }
}

public class GetFilterReconciliationSummariesQueryHandler : IRequestHandler<GetFilterReconciliationSummariesQuery, PaginatedList<ReconciliationSummaryDto>>
{
    private readonly IReconciliationService _reconciliationService;

    public GetFilterReconciliationSummariesQueryHandler(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task<PaginatedList<ReconciliationSummaryDto>> Handle(GetFilterReconciliationSummariesQuery request, CancellationToken cancellationToken)
    {
        return await _reconciliationService.GetFilterReconciliationSummariesAsync(request);
    }
}
