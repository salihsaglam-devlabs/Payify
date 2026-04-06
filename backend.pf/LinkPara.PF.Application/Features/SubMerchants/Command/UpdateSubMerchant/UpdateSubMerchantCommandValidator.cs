using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.UpdateSubMerchant;

public class UpdateSubMerchantCommandValidator : AbstractValidator<UpdateSubMerchantCommand>
{
    public UpdateSubMerchantCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
