using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.SaveSubMerchant;

public class SaveSubMerchantCommandValidator : AbstractValidator<SaveSubMerchantCommand>
{
    public SaveSubMerchantCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(150).WithMessage("Invalid Sub Merchant name!");

        RuleFor(x => x.MerchantType).IsInEnum();

        RuleFor(x => x.MerchantId).NotNull().NotEmpty();

        RuleFor(x => x.City).NotNull().NotEmpty();

        RuleFor(x => x.CityName).NotNull().NotEmpty();
    }
}
