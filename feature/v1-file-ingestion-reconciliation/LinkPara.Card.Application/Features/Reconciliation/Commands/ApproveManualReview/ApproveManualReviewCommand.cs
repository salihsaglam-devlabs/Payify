using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.ContextProvider;
using LinkPara.SystemUser;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.ApproveManualReview;

public class ApproveManualReviewCommand : IRequest<ManualReviewOperationResult>
{
    public Guid ManualReviewItemId { get; set; }
    public string Note { get; set; }
}

public class ApproveManualReviewCommandValidator : AbstractValidator<ApproveManualReviewCommand>
{
    public ApproveManualReviewCommandValidator()
    {
        RuleFor(x => x.ManualReviewItemId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class ApproveManualReviewCommandHandler : IRequestHandler<ApproveManualReviewCommand, ManualReviewOperationResult>
{
    private readonly IReconciliationManualOperationService _manualOperationService;
    private readonly IContextProvider _contextProvider;

    public ApproveManualReviewCommandHandler(
        IReconciliationManualOperationService manualOperationService,
        IContextProvider contextProvider)
    {
        _manualOperationService = manualOperationService;
        _contextProvider = contextProvider;
    }

    public Task<ManualReviewOperationResult> Handle(ApproveManualReviewCommand request, CancellationToken cancellationToken)
    {
        var actor = _contextProvider?.CurrentContext?.UserId ?? AuditUsers.CardFileIngestion;
        return _manualOperationService.ApproveManualReviewAsync(request.ManualReviewItemId, request.Note, actor, cancellationToken);
    }
}
