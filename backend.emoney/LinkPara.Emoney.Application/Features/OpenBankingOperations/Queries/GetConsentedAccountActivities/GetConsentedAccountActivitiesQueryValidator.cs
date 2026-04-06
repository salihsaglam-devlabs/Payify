using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountActivities;

public class GetConsentedAccountActivitiesQueryValidator : AbstractValidator<GetConsentedAccountActivitiesQuery>
{
    public GetConsentedAccountActivitiesQueryValidator()
    {
        RuleFor(x => x.AccountReference).NotEmpty().NotNull();
        RuleFor(x => x.HesapIslemBslTrh).NotEmpty().NotNull();
        RuleFor(x => x.HesapIslemBtsTrh).NotEmpty().NotNull();
    }
}
