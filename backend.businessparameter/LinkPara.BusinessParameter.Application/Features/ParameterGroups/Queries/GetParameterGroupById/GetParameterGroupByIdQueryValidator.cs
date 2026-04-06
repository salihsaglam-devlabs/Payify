using FluentValidation;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetParameterGroupById;
public class GetParameterGroupByIdQueryValidator : AbstractValidator<GetParameterGroupByIdQuery>
{
    public GetParameterGroupByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
