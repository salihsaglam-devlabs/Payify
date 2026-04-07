using FluentValidation;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.GetManualReviewDecisionPreview;

public class GetManualReviewDecisionPreviewQuery : IRequest<ManualReviewExecutionPreview>
{
    public Guid ManualReviewItemId { get; set; }
}

public class GetManualReviewDecisionPreviewQueryValidator : AbstractValidator<GetManualReviewDecisionPreviewQuery>
{
    public GetManualReviewDecisionPreviewQueryValidator()
    {
        RuleFor(x => x.ManualReviewItemId).NotEmpty();
    }
}

public class GetManualReviewDecisionPreviewQueryHandler : IRequestHandler<GetManualReviewDecisionPreviewQuery, ManualReviewExecutionPreview>
{
    private readonly IReconciliationManualOperationService _manualOperationService;

    public GetManualReviewDecisionPreviewQueryHandler(IReconciliationManualOperationService manualOperationService)
    {
        _manualOperationService = manualOperationService;
    }

    public Task<ManualReviewExecutionPreview> Handle(GetManualReviewDecisionPreviewQuery request, CancellationToken cancellationToken)
    {
        return _manualOperationService.GetManualReviewDecisionPreviewAsync(request.ManualReviewItemId, cancellationToken);
    }
}
