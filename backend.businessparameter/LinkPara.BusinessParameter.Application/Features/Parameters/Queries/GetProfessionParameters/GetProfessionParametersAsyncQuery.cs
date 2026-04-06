using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetProfessionParameters;
public class GetProfessionParametersAsyncQuery : IRequest<List<ParameterDto>>
{
}
public class GetProfessionParametersAsyncQueryHandler : IRequestHandler<GetProfessionParametersAsyncQuery, List<ParameterDto>>
{
    private readonly IParameterService _parameterService;
    private const string Profession = "Professions";
    public GetProfessionParametersAsyncQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<List<ParameterDto>> Handle(GetProfessionParametersAsyncQuery request, CancellationToken cancellationToken)
    {
        return await _parameterService.GetParametersAsync(Profession);
    }
}
