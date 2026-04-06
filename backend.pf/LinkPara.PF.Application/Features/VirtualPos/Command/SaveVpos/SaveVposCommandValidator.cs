using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;

public class SaveVposCommandValidator : AbstractValidator<SaveVposCommand>
{
    private readonly IStringLocalizer _localizer;
    public SaveVposCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        
        RuleFor(x => x.Name).MaximumLength(50)
           .WithMessage("Invalid Vpos name!");

        RuleFor(x => x.AcquireBankId).NotNull().NotEmpty()
            .WithMessage("Acquire Bank cant be empty!");

        RuleForEach(x => x.VposBankApiInfos)
          .SetValidator(new SaveBankApiKeyCommandValidator());
        
        RuleFor(x => 
                (x.IsOnUsVpos && (x.IsTopUpVpos == true || x.IsInsuranceVpos))
                || (x.IsInsuranceVpos && (x.IsOnUsVpos || x.IsTopUpVpos == true))
                || (x.IsTopUpVpos == true && (x.IsOnUsVpos || x.IsInsuranceVpos)))
            .Equal(false)
            .WithMessage(_localizer.GetString("MultipleVposTypeNotAllowed").Value);
    }
}

public class SaveCostProfileCommandValidator : AbstractValidator<CostProfileDto>
{
    public SaveCostProfileCommandValidator()
    {
        RuleFor(s => s.ActivationDate)
           .NotNull()
           .NotEmpty()
           .GreaterThan(DateTime.Now)
           .WithMessage("Invalid activation date!");

        RuleFor(s => s.ServiceCommission)
           .GreaterThanOrEqualTo(0);

        RuleFor(s => s.PointCommission)
           .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.CostProfileItems)
           .SetValidator(new SaveCostProfileItemCommandValidator());
    }
}

public class SaveCostProfileItemCommandValidator : AbstractValidator<CostProfileItemDto>
{
    public SaveCostProfileItemCommandValidator()
    {
        RuleFor(s => s.CommissionRate)
           .GreaterThanOrEqualTo(0);

        RuleFor(s => s.BlockedDayNumber)
           .GreaterThanOrEqualTo(0);
    }
}

public class SaveBankApiKeyCommandValidator : AbstractValidator<SaveBankApiInfoDto>
{
    public SaveBankApiKeyCommandValidator()
    {
        RuleFor(x => x.KeyId).NotNull().NotEmpty();

        RuleFor(x => x.Value).NotNull().NotEmpty();
    }
}
