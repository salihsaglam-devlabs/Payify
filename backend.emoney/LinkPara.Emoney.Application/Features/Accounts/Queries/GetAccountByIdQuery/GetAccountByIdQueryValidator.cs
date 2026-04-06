using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountByIdQuery;

public class GetAccountByIdQueryValidator : AbstractValidator<GetAccountByIdQuery>
{
    public GetAccountByIdQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}
