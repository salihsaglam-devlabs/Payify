using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Queries.GetCharges;

public class GetChargeTransactionsSearchQueryValidator : AbstractValidator<GetChargeTransactionsSearchQuery>
{
    public GetChargeTransactionsSearchQueryValidator()
    {
        RuleFor(b => b.TransactionDateEnd)
            .GreaterThan(b => b.TransactionDateStart.Value)
            .WithMessage("End date must after Start date!")
            .When(b => b.TransactionDateStart.HasValue);
    }
}
