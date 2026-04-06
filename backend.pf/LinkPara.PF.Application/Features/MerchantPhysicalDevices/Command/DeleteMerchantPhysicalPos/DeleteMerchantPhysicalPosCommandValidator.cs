using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.DeleteMerchantPhysicalPos;

public class DeleteMerchantPhysicalPosCommandValidator : AbstractValidator<DeleteMerchantPhysicalPosCommand>
{
    public DeleteMerchantPhysicalPosCommandValidator()
    {
        RuleFor(x => x.MerchantPhysicalPosId).NotNull().NotEmpty();
    }
}
