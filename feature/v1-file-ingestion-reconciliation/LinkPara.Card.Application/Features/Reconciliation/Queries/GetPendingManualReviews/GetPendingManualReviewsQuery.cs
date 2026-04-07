using FluentValidation;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.GetPendingManualReviews;

public class GetPendingManualReviewsQuery : IRequest<IReadOnlyCollection<ReconciliationRunListItem>>
{
    public int Take { get; set; } = 100;
}

public class GetPendingManualReviewsQueryValidator : AbstractValidator<GetPendingManualReviewsQuery>
{
    public GetPendingManualReviewsQueryValidator()
    {
        RuleFor(x => x.Take).GreaterThan(0);
    }
}

public class GetPendingManualReviewsQueryHandler : IRequestHandler<GetPendingManualReviewsQuery, IReadOnlyCollection<ReconciliationRunListItem>>
{
    private readonly IReconciliationManualOperationService _manualOperationService;

    public GetPendingManualReviewsQueryHandler(IReconciliationManualOperationService manualOperationService)
    {
        _manualOperationService = manualOperationService;
    }

    public Task<IReadOnlyCollection<ReconciliationRunListItem>> Handle(GetPendingManualReviewsQuery request, CancellationToken cancellationToken)
    {
        return _manualOperationService.GetPendingManualReviewsAsync(request.Take, cancellationToken);
    }
}
