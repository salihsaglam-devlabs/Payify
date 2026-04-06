using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountDetailQuery;

public class GetAccountDetailQueryValidator : AbstractValidator<GetAccountDetailQuery>
{
    public GetAccountDetailQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.WalletNumber));

        RuleFor(x => x.WalletNumber)
            .NotNull()
            .NotEmpty()
            .MaximumLength(10)
            .When(x => x.UserId == Guid.Empty);
    }
}
