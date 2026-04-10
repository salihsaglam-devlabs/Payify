using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews
{
    public class GetPendingReviewsQuery : IRequest<PendingReviewsResponse>
    {
        public PendingReviewsRequest Request { get; set; } = new PendingReviewsRequest();
    }

    public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, PendingReviewsResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public GetPendingReviewsQueryHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<PendingReviewsResponse> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetPendingReviewsAsync(request.Request, cancellationToken);
        }
    }
}
