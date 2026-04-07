using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.ContextProvider;
using LinkPara.SystemUser;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.RegenerateOperations;

public class RegenerateOperationsCommand : IRequest<ReconciliationProcessSummary>
{
    public int Take { get; set; } = 1000;
    public Guid? ImportedFileId { get; set; }
    public int? LookbackDays { get; set; }
    public bool IncludeDetails { get; set; }
    public bool VerifyWithDbAudit { get; set; }
}

public class RegenerateOperationsCommandValidator : AbstractValidator<RegenerateOperationsCommand>
{
    public RegenerateOperationsCommandValidator()
    {
        RuleFor(x => x.Take).GreaterThan(0);
        RuleFor(x => x.LookbackDays!.Value).GreaterThan(0).When(x => x.LookbackDays.HasValue);
    }
}

public class RegenerateOperationsCommandHandler : IRequestHandler<RegenerateOperationsCommand, ReconciliationProcessSummary>
{
    private readonly IReconciliationService _reconciliationService;
    private readonly IContextProvider _contextProvider;

    public RegenerateOperationsCommandHandler(
        IReconciliationService reconciliationService,
        IContextProvider contextProvider)
    {
        _reconciliationService = reconciliationService;
        _contextProvider = contextProvider;
    }

    public Task<ReconciliationProcessSummary> Handle(RegenerateOperationsCommand request, CancellationToken cancellationToken)
    {
        var actor = _contextProvider?.CurrentContext?.UserId ?? AuditUsers.CardFileIngestion;
        return _reconciliationService.RegenerateOperationsAsync(
            actor,
            request.Take,
            request.ImportedFileId,
            request.LookbackDays,
            new ReconciliationSummaryOptions
            {
                IncludeDetails = request.IncludeDetails,
                VerifyWithDbAudit = request.VerifyWithDbAudit
            },
            cancellationToken: cancellationToken);
    }
}
