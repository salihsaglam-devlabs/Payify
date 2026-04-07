using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class ExecuteCommandValidator : AbstractValidator<ExecuteCommand>
{
    public ExecuteCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull();

        RuleForEach(x => x.Request.GroupIds)
            .NotEqual(Guid.Empty);

        RuleForEach(x => x.Request.OperationIds)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Request.GroupIds)
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .When(x => x.Request.GroupIds.Length > 0)
            .WithMessage("GroupIds must be distinct.");

        RuleFor(x => x.Request.OperationIds)
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .When(x => x.Request.OperationIds.Length > 0)
            .WithMessage("OperationIds must be distinct.");

        When(x => x.Request.Options is not null, () =>
        {
            RuleFor(x => x.Request.Options!.MaxEvaluations)
                .InclusiveBetween(1, 5000);

            RuleFor(x => x.Request.Options!.LeaseSeconds)
                .InclusiveBetween(1, 3600);
        });
    }
}
