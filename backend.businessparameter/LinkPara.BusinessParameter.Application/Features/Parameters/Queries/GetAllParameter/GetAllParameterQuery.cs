using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;
public class GetAllParameterQuery : SearchQueryParams, IRequest<PaginatedList<ParameterDto>>
{
    public string GroupCode { get; set; }
}
public class GetAllParameterQueryHandler : IRequestHandler<GetAllParameterQuery, PaginatedList<ParameterDto>>
{
    private readonly IParameterService _parameterService;

    public GetAllParameterQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<PaginatedList<ParameterDto>> Handle(GetAllParameterQuery request, CancellationToken cancellationToken)
    {
        return await _parameterService.GetAllParameterAsync(request);
    }
}