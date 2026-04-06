using FluentValidation;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;

namespace LinkPara.PF.Application.Features.CostProfiles.Command.UpdatePreviewCostProfile;

public class UpdatePreviewCostProfileCommandValidator : AbstractValidator<UpdatePreviewCostProfileCommand>
{
    public UpdatePreviewCostProfileCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.CostProfileItems)
            .SetValidator(new SaveCostProfileItemCommandValidator());
    }
}