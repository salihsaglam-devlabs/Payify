using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;

public class SaveMerchantIntegratorCommandValidator : AbstractValidator<SaveMerchantIntegratorCommand>
{
    public SaveMerchantIntegratorCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100)
           .WithMessage("Invalid profile name!");

        RuleFor(s => s.CommissionRate)
          .GreaterThanOrEqualTo(0);
    }
}
