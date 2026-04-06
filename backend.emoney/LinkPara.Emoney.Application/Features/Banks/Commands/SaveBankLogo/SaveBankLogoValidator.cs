using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Banks.Commands.SaveBankLogo;

public class SaveBankLogoCommandValidator : AbstractValidator<SaveBankLogoCommand>
{
    public SaveBankLogoCommandValidator()
    {
        RuleFor(s => s.BankLogo.ContentType)
            .MaximumLength(100);
    }
}
