using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;

public class GetParameterTemplateValueByIdQuery : IRequest<ParameterTemplateValueDto>
{
    public Guid Id { get; set; }
}

public class GetParameterTemplateValueByIdQueryHandler : IRequestHandler<GetParameterTemplateValueByIdQuery, ParameterTemplateValueDto>
{
    private readonly IParameterTemplateValueService _parameterTemplateValueService;

    public GetParameterTemplateValueByIdQueryHandler(IParameterTemplateValueService parameterTemplateValueService)
    {
        _parameterTemplateValueService = parameterTemplateValueService;
    }
    public async Task<ParameterTemplateValueDto> Handle(GetParameterTemplateValueByIdQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateValueService.GetByIdAsync(request);
    }
}
