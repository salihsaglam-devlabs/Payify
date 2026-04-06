using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
public class GetParameterTemplateByIdQueryValidator : AbstractValidator<GetParameterTemplateByIdQuery>
{
    public GetParameterTemplateByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
