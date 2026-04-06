using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
public class SaveParameterGroupCommandValidator : AbstractValidator<SaveParameterGroupCommand>
{
    public SaveParameterGroupCommandValidator()
    {
        RuleFor(x => x.Explanation).NotNull().NotEmpty();
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
    }
}
