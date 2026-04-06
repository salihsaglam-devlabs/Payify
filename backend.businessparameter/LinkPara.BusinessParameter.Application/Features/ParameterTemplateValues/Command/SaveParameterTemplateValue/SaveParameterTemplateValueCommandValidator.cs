using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
public class SaveParameterTemplateValueCommandValidator : AbstractValidator<SaveParameterTemplateValueCommand>
{
    public SaveParameterTemplateValueCommandValidator()
    {
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.TemplateCode).NotNull().NotEmpty();
        When(b => String.IsNullOrEmpty(b.ParameterCode), () =>
        {
            RuleFor(b => b.ParameterValue).NotNull().NotEmpty()
               .WithMessage("ParameterValue not null!");
        });

        When(b => String.IsNullOrEmpty(b.ParameterValue), () =>
        {
            RuleFor(b => b.ParameterCode).NotNull().NotEmpty()
               .WithMessage("ParameterCode not null!");
        });
    }
}

