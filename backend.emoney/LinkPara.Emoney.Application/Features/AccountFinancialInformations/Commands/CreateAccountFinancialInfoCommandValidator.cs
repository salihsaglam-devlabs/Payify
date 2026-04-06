using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountFinancialInformations.Commands;

public class CreateAccountFinancialInfoCommandValidator : AbstractValidator<CreateAccountFinancialInfoCommand>
{
	public CreateAccountFinancialInfoCommandValidator()
	{
		RuleFor(s => s.IncomeSource)
			.NotEmpty()
			.NotEmpty()
			.MaximumLength(50);

        RuleFor(s => s.IncomeInformation)
            .NotEmpty()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(s => s.MonthlyTransactionVolume)
            .NotEmpty()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(s => s.MonthlyTransactionCount)
            .NotEmpty()
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(s => s.AccountId)
            .NotEmpty()
            .NotEmpty();
    }
}
