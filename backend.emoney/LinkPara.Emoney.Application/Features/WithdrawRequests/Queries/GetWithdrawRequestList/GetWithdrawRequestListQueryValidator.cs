using FluentValidation;

namespace LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestList;

public class GetWithdrawRequestListQueryValidator : AbstractValidator<GetWithdrawRequestListQuery>
{
    public GetWithdrawRequestListQueryValidator()
    {
        RuleFor(s => s.WalletNumber)
            .MaximumLength(10);

        RuleFor(s => s.Description)
             .MaximumLength(150);

        RuleFor(s => s.CurrencyCode)
          .MaximumLength(3);
    }
}