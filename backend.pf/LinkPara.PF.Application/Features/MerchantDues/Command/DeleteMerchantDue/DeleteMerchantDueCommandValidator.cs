using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantDues.Command.DeleteMerchantDue;

public class DeleteMerchantDueCommandValidator : AbstractValidator<DeleteMerchantDueCommand>
{
    public DeleteMerchantDueCommandValidator()
    {
        RuleFor(b => b.Id)
            .NotNull()
            .NotEmpty();
    }
}