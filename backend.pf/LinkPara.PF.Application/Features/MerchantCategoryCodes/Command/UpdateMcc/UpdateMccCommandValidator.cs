using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;

public class UpdateMccCommandValidator : AbstractValidator<UpdateMccCommand>
{
    public UpdateMccCommandValidator()
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
