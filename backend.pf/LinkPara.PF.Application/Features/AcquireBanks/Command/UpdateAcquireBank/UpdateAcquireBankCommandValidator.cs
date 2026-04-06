using FluentValidation;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;

public class UpdateAcquireBankCommandValidator : AbstractValidator<UpdateAcquireBankCommand>
{
    public UpdateAcquireBankCommandValidator()
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
