using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountFinancialInformations.Queries;

public class GetAccountFinancialInfoQueryValidator : AbstractValidator<GetAccountFinancialInfoQuery>
{
	public GetAccountFinancialInfoQueryValidator()
	{
		RuleFor(s => s.AccountId)
			.NotNull()
			.NotEmpty();
	}
}
