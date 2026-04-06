using FluentValidation;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;

public class SavePricingProfileCommandValidator : AbstractValidator<SavePricingProfileCommand>
{
    private readonly IStringLocalizer _localizer;
    public SavePricingProfileCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(s => s.ActivationDateStart)
            .NotNull()
            .NotEmpty()
            .GreaterThan(DateTime.Now)
            .WithMessage(_localizer.GetString("InvalidActivationDateException").Value);

        RuleFor(s => s.TransferType)
            .IsInEnum();

        RuleForEach(x => x.ProfileItems)
            .SetValidator(new PricingProfileItemValidator());
    }
}

public class PricingProfileItemValidator : AbstractValidator<PricingProfileItemModel>
{
    public PricingProfileItemValidator()
    {
        RuleFor(s => s.Fee)
             .GreaterThanOrEqualTo(0);

        RuleFor(s => s.CommissionRate)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MinAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MaxAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.WalletType)
            .IsInEnum();

        RuleFor(s => s).Must(CheckAmount).WithMessage("MaxAmount must be greater then MinAmount");
    }

    public static bool CheckAmount(PricingProfileItemModel model)
    {
        return model.MaxAmount >= model.MinAmount;
    }
}
