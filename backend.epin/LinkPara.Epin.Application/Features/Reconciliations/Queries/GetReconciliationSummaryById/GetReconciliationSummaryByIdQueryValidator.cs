using FluentValidation;
namespace LinkPara.Epin.Application.Features.Reconciliations.Queries.GetReconciliationSummaryById;

public class GetReconciliationSummaryByIdQueryValidator : AbstractValidator<GetReconciliationSummaryByIdQuery>
{
    public GetReconciliationSummaryByIdQueryValidator()
    {
        RuleFor(b => b.ReconciliationSummaryId)
            .NotNull()
            .NotEmpty();
    }
}
