using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;

public class DeleteMccCommandValidator : AbstractValidator<DeleteMccCommand>
{
    public DeleteMccCommandValidator()
    {
        RuleFor(x => x.Id)
       .NotNull().NotEmpty();
    }
}
