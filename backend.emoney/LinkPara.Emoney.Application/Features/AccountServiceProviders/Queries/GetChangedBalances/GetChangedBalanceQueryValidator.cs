using FluentValidation;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalance;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalances;

public class GetChangedBalanceQueryValidator : AbstractValidator<GetChangedBalanceQuery>
{
    public GetChangedBalanceQueryValidator()
    {
        
    }
}
