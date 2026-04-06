using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;
public class SaveParameterCommandValidator : AbstractValidator<SaveParameterCommand>
{
    public SaveParameterCommandValidator()
    {
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.ParameterCode).NotNull().NotEmpty();
        RuleFor(x => x.ParameterValue).NotNull().NotEmpty();
    }
}
