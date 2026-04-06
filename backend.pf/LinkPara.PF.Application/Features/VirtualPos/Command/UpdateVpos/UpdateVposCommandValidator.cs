using FluentValidation;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;

public class UpdateVposCommandValidator : AbstractValidator<UpdateVposCommand>
{
    private readonly IStringLocalizer _localizer;
    
    public UpdateVposCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        
        RuleFor(x => x.Name).MaximumLength(50)
          .WithMessage("Invalid Vpos name!");

        RuleFor(x => x.AcquireBankId).NotNull().NotEmpty();

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
