using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;

public class GetHhsAccessTokenQueryValidator : AbstractValidator<GetHhsAccessTokenQuery>
{
    public GetHhsAccessTokenQueryValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.ConsentType).NotEmpty().NotNull();
        RuleFor(x => x.AccessCode).NotEmpty().NotNull();
    }
}
