using FluentValidation;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;

public class DeleteAcquireBankCommandValidator : AbstractValidator<DeleteAcquireBankCommand>
{
    public DeleteAcquireBankCommandValidator()
    {
        RuleFor(x => x.Id)
      .NotNull().NotEmpty();
    }
}
