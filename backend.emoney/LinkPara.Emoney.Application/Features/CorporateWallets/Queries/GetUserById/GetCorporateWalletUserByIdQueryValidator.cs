using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetUserById;

public class GetCorporateWalletUserByIdQueryValidator : AbstractValidator<GetCorporateWalletUserByIdQuery>
{
    public GetCorporateWalletUserByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
