using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionSummary;

public class GetTransactionSummaryQueryValidator : AbstractValidator<GetTransactionSummaryQuery>
{
    public GetTransactionSummaryQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.EndDate)
         .NotNull()
         .NotEmpty();

    }
}