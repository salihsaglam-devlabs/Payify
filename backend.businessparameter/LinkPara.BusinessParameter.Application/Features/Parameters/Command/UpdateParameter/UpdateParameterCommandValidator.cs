using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;
public class UpdateParameterCommandValidator : AbstractValidator<UpdateParameterCommand>
{
    public UpdateParameterCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
        RuleFor(x => x.ParameterValue).NotNull().NotEmpty();
    }
}
