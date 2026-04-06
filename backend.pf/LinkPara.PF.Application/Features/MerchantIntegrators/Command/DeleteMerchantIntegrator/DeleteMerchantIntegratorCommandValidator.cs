using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.DeleteMerchantIntegrator;

public class DeleteMerchantIntegratorCommandValidator : AbstractValidator<DeleteMerchantIntegratorCommand>
{
    public DeleteMerchantIntegratorCommandValidator()
    {
        RuleFor(x => x.Id)
       .NotNull().NotEmpty();
    }
}
