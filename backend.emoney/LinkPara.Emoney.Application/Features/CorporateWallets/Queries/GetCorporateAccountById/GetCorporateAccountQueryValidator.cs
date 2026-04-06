using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetCorporateAccountById;

public class GetCorporateAccountQueryValidator : AbstractValidator<GetCorporateAccountByIdQuery>
{
    public GetCorporateAccountQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
