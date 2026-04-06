using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;

public class ApproveMerchantPoolCommandValidator : AbstractValidator<ApproveMerchantPoolCommand>
{
    public ApproveMerchantPoolCommandValidator()
    {
        RuleFor(b => b.MerchantPoolId).NotEmpty().NotNull();

        When(b => b.IsApprove == false, () =>
        {
            When(b => b.ParameterValue is null, () =>
            {
                RuleFor(b => b.RejectReason).NotNull().NotEmpty()
                   .WithMessage("Reject reason not null!");
            });
        });
    }
}
