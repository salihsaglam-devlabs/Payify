using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveBankAccount;

public class SaveBankAccountCommandValidator : AbstractValidator<SaveBankAccountCommand>
{
    private readonly IStringLocalizer _localizer;
    public SaveBankAccountCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
        
        RuleFor(x => x.UserId)
                .NotEmpty()
                .NotNull();

        RuleFor(x => x.Iban)
            .NotEmpty()
            .NotNull()
            .MinimumLength(24)
            .MaximumLength(50);

        RuleFor(x => x.Tag)
            .NotEmpty()
            .NotNull()
            .MinimumLength(3)
            .MaximumLength(20)
            .WithMessage(_localizer.GetString("InvalidTagLengthException").Value);
    }
}
