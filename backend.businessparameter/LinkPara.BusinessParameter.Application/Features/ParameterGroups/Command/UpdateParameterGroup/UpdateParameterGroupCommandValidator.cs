using FluentValidation;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
public class UpdateParameterGroupCommandValidator : AbstractValidator<UpdateParameterGroupCommand>
{
    public UpdateParameterGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.Explanation).NotNull().NotEmpty();
    }
}

