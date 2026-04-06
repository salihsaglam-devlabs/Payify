using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.UpdateMerchantIntegrator;

public class UpdateMerchantIntegratorCommandValidator : AbstractValidator<UpdateMerchantIntegratorCommand>
{
    public UpdateMerchantIntegratorCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100)
           .WithMessage("Invalid profile name!");

        RuleFor(s => s.CommissionRate)
          .GreaterThanOrEqualTo(0);
    }
}
