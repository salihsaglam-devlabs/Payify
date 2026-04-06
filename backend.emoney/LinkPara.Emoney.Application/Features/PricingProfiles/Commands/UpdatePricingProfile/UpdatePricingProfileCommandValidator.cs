using FluentValidation;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;

public class UpdatePricingProfileCommandValidator : AbstractValidator<UpdatePricingProfileCommand>
{
    private readonly IStringLocalizer _localizer;
    public UpdatePricingProfileCommandValidator(IStringLocalizerFactory factory)
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

public class PricingProfileItemValidator : AbstractValidator<PricingProfileItemUpdateModel>
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

    public static bool CheckAmount(PricingProfileItemUpdateModel model)
    {
        return model.MaxAmount >= model.MinAmount;
    }
}