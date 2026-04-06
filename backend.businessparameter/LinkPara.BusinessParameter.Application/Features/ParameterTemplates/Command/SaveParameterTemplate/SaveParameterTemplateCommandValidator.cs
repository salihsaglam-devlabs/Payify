using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
public class SaveParameterTemplateCommandValidator : AbstractValidator<SaveParameterTemplateCommand>
{
    public SaveParameterTemplateCommandValidator()
    {
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.TemplateCode).NotNull().NotEmpty();
        RuleFor(x => x.DataLength).NotNull().NotEmpty();
        RuleFor(x => x.DataType).IsInEnum();
    }
}