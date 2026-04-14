using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews
{
    public class GetPendingReviewsQuery : SearchQueryParams, IRequest<PaginatedList<ManualReview>>
    {
        public DateOnly? Date { get; set; }
    }

    public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, PaginatedList<ManualReview>>
    {
        private readonly IReconciliationService _service;

        public GetPendingReviewsQueryHandler(IReconciliationService service)
        {
            _service = service;
        }

        public async Task<PaginatedList<ManualReview>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetPendingReviewsAsync(request, cancellationToken);
        }
    }
}
