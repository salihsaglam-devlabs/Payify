using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionSummary;

public class GetInstitutionSummaryQuery : SearchQueryParams, IRequest<PaginatedList<InstitutionSummaryDto>>
{
    public Guid? VendorId { get; set; }
    public Guid? InstitutionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}

public class GetInstitutionSummaryQueryHandler : IRequestHandler<GetInstitutionSummaryQuery, PaginatedList<InstitutionSummaryDto>>
{
    private readonly IReconciliationService _reconciliationService;

    public GetInstitutionSummaryQueryHandler(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task<PaginatedList<InstitutionSummaryDto>> Handle(GetInstitutionSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _reconciliationService.GetInstitutionSummaryAsync(request);
    }
}