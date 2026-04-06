using FluentValidation;

namespace LinkPara.Epin.Application.Features.Reconciliations.Commands.ReconciliationByDate;

public class ReconciliationByDateCommandValidator : AbstractValidator<ReconciliationByDateCommand>
{
    public ReconciliationByDateCommandValidator()
    {
        RuleFor(x => x.ReconciliationDate)
            .NotNull().NotEmpty();
    }
}
