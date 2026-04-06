using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;

public class DeleteParameterTemplateCommandValidator : AbstractValidator<DeleteParameterTemplateCommand>
{
    public DeleteParameterTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
