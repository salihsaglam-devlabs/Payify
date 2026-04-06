using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Banks.Queries.GetBanksList;

public class GetBanksListQueryValidator : AbstractValidator<GetBanksListQuery>
{
    public GetBanksListQueryValidator()
    {
    }
}