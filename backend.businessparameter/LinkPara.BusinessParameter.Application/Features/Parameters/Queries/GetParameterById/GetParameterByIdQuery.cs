using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameterById;
public class GetParameterByIdQuery : IRequest<ParameterDto>
{
    public Guid Id { get; set; }
}

public class GetParameterByIdQueryHandler : IRequestHandler<GetParameterByIdQuery, ParameterDto>
{
    private readonly IParameterService _parameterService;

    public GetParameterByIdQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<ParameterDto> Handle(GetParameterByIdQuery request, CancellationToken cancellationToken)
    {
        return await _parameterService.GetByIdAsync(request);
    }
}

