using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Queries.GetInstitutionDetail;

public class GetInstitutionDetailQuery : SearchQueryParams, IRequest<PaginatedList<InstitutionDetailDto>>
{
    public Guid InstitutionSummaryId { get; set; }
}

public class GetInstitutionDetailQueryHandler : IRequestHandler<GetInstitutionDetailQuery, PaginatedList<InstitutionDetailDto>>
{
    private readonly IReconciliationService _reconciliationService;

    public GetInstitutionDetailQueryHandler(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task<PaginatedList<InstitutionDetailDto>> Handle(GetInstitutionDetailQuery request, CancellationToken cancellationToken)
    {
        return await _reconciliationService.GetInstitutionDetailAsync(request);
    }
}