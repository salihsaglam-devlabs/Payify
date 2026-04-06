using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletSummaries;

public class GetWalletSummaryQueryValidator : AbstractValidator<GetWalletSummaryQuery>
{ 
    public GetWalletSummaryQueryValidator()
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