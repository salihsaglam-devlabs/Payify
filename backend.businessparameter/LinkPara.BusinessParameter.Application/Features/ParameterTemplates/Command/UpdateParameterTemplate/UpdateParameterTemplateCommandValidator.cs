using FluentValidation;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
public class UpdateParameterTemplateCommandValidator : AbstractValidator<UpdateParameterTemplateCommand>
{
    public UpdateParameterTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
        RuleFor(x => x.GroupCode).NotNull().NotEmpty();
        RuleFor(x => x.TemplateCode).NotNull().NotEmpty();
        RuleFor(x => x.DataLength).NotNull().NotEmpty();
        RuleFor(x => x.DataType).IsInEnum();
    }
}


