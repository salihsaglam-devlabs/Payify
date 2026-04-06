using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.DeleteSubMerchant;

public class DeleteSubMerchantCommandValidator : AbstractValidator<DeleteSubMerchantCommand>
{
    public DeleteSubMerchantCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}