using LinkPara.Epin.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Epin.Application.Features.Reconciliations.Queries.GetReconciliationSummaryById;

public class GetReconciliationSummaryByIdQuery : IRequest<ReconciliationSummaryDto>
{
    public Guid ReconciliationSummaryId { get; set; }
}

public class GetReconciliationSummaryByIdQueryHandler : IRequestHandler<GetReconciliationSummaryByIdQuery, ReconciliationSummaryDto>
{
    private readonly IReconciliationService _reconciliationService;

    public GetReconciliationSummaryByIdQueryHandler(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public async Task<ReconciliationSummaryDto> Handle(GetReconciliationSummaryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _reconciliationService.GetReconciliationSummaryByIdAsync(request);
    }
}
