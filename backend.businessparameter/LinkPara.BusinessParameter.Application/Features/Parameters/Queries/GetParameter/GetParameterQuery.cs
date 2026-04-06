using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameter;

public class GetParameterQuery : IRequest<ParameterDto>
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
}

public class GetParameterQueryHandler : IRequestHandler<GetParameterQuery, ParameterDto>
{
    private readonly IParameterService _parameterService;

    public GetParameterQueryHandler(IGenericRepository<Parameter> repository, IParameterService parameterService)
    {
        _parameterService = parameterService;
    }

    public async Task<ParameterDto> Handle(GetParameterQuery request, CancellationToken cancellationToken)
    {
        return await _parameterService.GetParameterAsync(request.GroupCode, request.ParameterCode);
    }
}
