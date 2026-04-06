using FluentValidation;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;

public class SaveAcquireBankCommandValidator : AbstractValidator<SaveAcquireBankCommand>
{
    public SaveAcquireBankCommandValidator()
    {
        RuleFor(x => x.BankCode)
        .NotNull().NotEmpty().WithMessage("Bank code cant be empty!");

        RuleFor(x => x.EndOfDayHour)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EndOfDayMinute)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CardNetwork)
            .IsInEnum();
    }
}
