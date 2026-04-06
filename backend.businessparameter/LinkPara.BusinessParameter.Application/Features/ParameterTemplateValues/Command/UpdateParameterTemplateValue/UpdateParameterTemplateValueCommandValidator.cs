using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.UpdateParameterTemplateValue;
public class UpdateParameterTemplateValueCommandValidator : AbstractValidator<UpdateParameterTemplateValueCommand>
{
    public UpdateParameterTemplateValueCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.TemplateCode).NotNull().NotEmpty();
    }
}

