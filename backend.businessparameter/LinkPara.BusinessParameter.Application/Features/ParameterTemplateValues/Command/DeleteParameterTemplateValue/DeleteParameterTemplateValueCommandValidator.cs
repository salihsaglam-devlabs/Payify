using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.DeleteParameterTemplateValue;
public class DeleteParameterTemplateValueCommandValidator : AbstractValidator<DeleteParameterTemplateValueCommand>
{
    public DeleteParameterTemplateValueCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}