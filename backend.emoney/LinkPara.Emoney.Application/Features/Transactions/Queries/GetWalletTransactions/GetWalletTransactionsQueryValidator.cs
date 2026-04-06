using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryValidator : AbstractValidator<GetWalletTransactionsQuery>
{
    private readonly IStringLocalizer _localizer;
    public GetWalletTransactionsQueryValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.WalletId).NotNull().NotEmpty();
        RuleFor(x => x.StartDate.Ticks)
            .GreaterThanOrEqualTo(x => x.EndDate.AddMonths(-12).Ticks)
            .WithMessage(_localizer.GetString("InvalidDateRange").Value);
    }
}