using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetAccountConsent;

public class GetAccountConsentQueryValidator : AbstractValidator<GetAccountConsentQuery>
{
    public GetAccountConsentQueryValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
    }
}
