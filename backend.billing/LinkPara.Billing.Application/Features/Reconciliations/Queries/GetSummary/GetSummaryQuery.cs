using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetSummary;

public class GetSummaryQuery : SearchQueryParams, IRequest<PaginatedList<SummaryDto>>
{
    public Guid? VendorId { get; set; }
    public DateTime ReconciliationStartDate { get; set; }
    public DateTime ReconciliationEndDate { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}

public class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, PaginatedList<SummaryDto>>
{
    private readonly IReconciliationService _reconciliationService;

    public GetSummaryQueryHandler(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task<PaginatedList<SummaryDto>> Handle(GetSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _reconciliationService.GetSummaryAsync(request);
    }
}