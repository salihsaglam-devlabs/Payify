using FluentValidation;

namespace LinkPara.PF.Application.Features.BankLimits.Command.DeleteBankLimit;

public class DeleteBankLimitCommandValidator : AbstractValidator<DeleteBankLimitCommand>
{
    public DeleteBankLimitCommandValidator()
    {
        RuleFor(x => x.Id)
      .NotNull().NotEmpty();
    }
}
