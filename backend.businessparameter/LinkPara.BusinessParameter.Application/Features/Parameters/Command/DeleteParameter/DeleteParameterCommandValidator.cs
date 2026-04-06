using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.DeleteParameter;
public class DeleteParameterCommandValidator : AbstractValidator<DeleteParameterCommand>
{
    public DeleteParameterCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}