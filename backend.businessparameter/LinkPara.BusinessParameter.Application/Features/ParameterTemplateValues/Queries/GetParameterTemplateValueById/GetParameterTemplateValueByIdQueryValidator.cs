using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;

public class GetParameterTemplateValueByIdQueryValidator : AbstractValidator<GetParameterTemplateValueByIdQuery>
{
    public GetParameterTemplateValueByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
