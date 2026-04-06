using FluentValidation;

namespace LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;

public class DeleteMerchantCommandValidator : AbstractValidator<DeleteMerchantCommand>
{
    public DeleteMerchantCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
