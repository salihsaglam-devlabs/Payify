using FluentValidation;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.DeleteSavedAccount;

public class DeleteSavedAccountCommandValidator : AbstractValidator<DeleteSavedAccountCommand>
{
    public DeleteSavedAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}