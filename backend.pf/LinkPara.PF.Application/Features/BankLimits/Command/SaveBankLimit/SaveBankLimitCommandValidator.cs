using FluentValidation;

namespace LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;

public class SaveBankLimitCommandValidator : AbstractValidator<SaveBankLimitCommand>
{
    public SaveBankLimitCommandValidator()
    {
        RuleFor(x => x.AcquireBankId)
              .NotNull().NotEmpty().WithMessage("Bank code cant be empty!");

        RuleFor(x => x.MonthlyLimitAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MarginRatio)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BankLimitType)
            .IsInEnum();

        RuleFor(x => x.LastValidDate)
            .NotNull().NotEmpty().WithMessage("Last Valid Date cant be empty!");
    }
}
