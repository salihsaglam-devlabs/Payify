using FluentValidation;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;

public class SaveCostProfileCommandValidator : AbstractValidator<SaveCostProfileCommand>
{
    private readonly IStringLocalizer _localizer;
    public SaveCostProfileCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");

        RuleFor(x => x.Name).MaximumLength(50)
          .WithMessage("Invalid CostProfile name!");

        RuleFor(s => s.ServiceCommission)
          .GreaterThanOrEqualTo(0);

        RuleFor(s => s.PointCommission)
           .GreaterThanOrEqualTo(0);

        RuleFor(s => s.ActivationDate)
           .NotNull()
           .NotEmpty()
           .GreaterThan(DateTime.Now)
           .WithMessage(_localizer.GetString("InvalidActivationDateException").Value);
       
        When(b => b.PosType == PosType.Virtual, () =>
        {
            RuleFor(s => s.VposId).NotNull().NotEmpty();
        });

        When(b => b.PosType == PosType.Physical, () =>
        {
            RuleFor(s => s.PhysicalPosId).NotNull().NotEmpty();
        });

        RuleForEach(x => x.CostProfileItems)
           .SetValidator(new SaveCostProfileItemCommandValidator());
    }
}
