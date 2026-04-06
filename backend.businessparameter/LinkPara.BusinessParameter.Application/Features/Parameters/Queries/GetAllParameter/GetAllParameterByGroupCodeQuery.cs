using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;

public class GetAllParameterByGroupCodeQuery : IRequest<List<ParameterDto>>
{
    public string GroupCode { get; set; }
}

public class GetAllParameterByGroupCodeQueryHandler : IRequestHandler<GetAllParameterByGroupCodeQuery, List<ParameterDto>>
{
    private readonly IParameterService _parameterService;

    public GetAllParameterByGroupCodeQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<List<ParameterDto>> Handle(GetAllParameterByGroupCodeQuery request, CancellationToken cancellationToken)
    {
        return await _parameterService.GetParametersAsync(request.GroupCode);
    }
}
