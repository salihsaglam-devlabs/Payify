using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateBankAccount;


public class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    private readonly IStringLocalizer _localizer;
    public UpdateBankAccountCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
        
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Tag)
            .NotEmpty()
            .NotNull()
            .MinimumLength(3)
            .MaximumLength(20)
            .WithMessage(_localizer.GetString("InvalidTagLengthException").Value);
        
        RuleFor(x => x.Iban)
             .NotNull()
             .NotEmpty()
             .MaximumLength(100);
    }
}

