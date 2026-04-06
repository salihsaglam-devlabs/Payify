using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;

public class SaveMccCommandValidator : AbstractValidator<SaveMccCommand>
{
    public SaveMccCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotNull().NotEmpty().Length(4)
            .Matches(@"[0-9]+$")
            .WithMessage("Invalid bin number!");

        RuleFor(x => x.Name)
         .NotNull().NotEmpty().WithMessage("Name cant be empty!");

        RuleFor(x => x.MaxCorporateInstallmentCount)
        .NotNull();

        RuleFor(x => x.MaxIndividualInstallmentCount)
        .NotNull();
    }
}
