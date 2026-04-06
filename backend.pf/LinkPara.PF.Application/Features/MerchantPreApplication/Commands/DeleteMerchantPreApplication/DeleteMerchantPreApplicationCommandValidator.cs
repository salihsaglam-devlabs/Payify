using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.DeleteMerchantPreApplication;

public class DeleteMerchantPreApplicationCommandValidator : AbstractValidator<DeleteMerchantPreApplicationCommand>
{
    public DeleteMerchantPreApplicationCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}