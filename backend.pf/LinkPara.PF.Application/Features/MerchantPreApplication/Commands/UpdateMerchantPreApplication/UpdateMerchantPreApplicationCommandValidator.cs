using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.UpdateMerchantPreApplication;

public class UpdateMerchantPreApplicationCommandValidator : AbstractValidator<UpdateMerchantPreApplicationCommand>
{
    public UpdateMerchantPreApplicationCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}