using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.ContextProvider;
using LinkPara.SystemUser;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.ExecutePendingOperations;

public class ExecutePendingOperationsCommand : IRequest<ReconciliationExecutionSummary>
{
    public int Take { get; set; } = 100;
}

public class ExecutePendingOperationsCommandValidator : AbstractValidator<ExecutePendingOperationsCommand>
{
    public ExecutePendingOperationsCommandValidator()
    {
        RuleFor(x => x.Take).GreaterThan(0);
    }
}

public class ExecutePendingOperationsCommandHandler : IRequestHandler<ExecutePendingOperationsCommand, ReconciliationExecutionSummary>
{
    private readonly IReconciliationAutoOperationService _autoOperationService;
    private readonly IContextProvider _contextProvider;

    public ExecutePendingOperationsCommandHandler(
        IReconciliationAutoOperationService autoOperationService,
        IContextProvider contextProvider)
    {
        _autoOperationService = autoOperationService;
        _contextProvider = contextProvider;
    }

    public Task<ReconciliationExecutionSummary> Handle(ExecutePendingOperationsCommand request, CancellationToken cancellationToken)
    {
        var actor = _contextProvider?.CurrentContext?.UserId ?? AuditUsers.CardFileIngestion;
        return _autoOperationService.ExecutePendingOperationsAsync(request.Take, actor, cancellationToken);
    }
}
