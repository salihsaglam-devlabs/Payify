using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;


namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetAllParameterTemplate;
public class GetAllParameterTemplateQuery : SearchQueryParams, IRequest<PaginatedList<ParameterTemplateDto>>
{
    public string GroupCode { get; set; }
}

public class GetAllParameterTemplateQueryHandler : IRequestHandler<GetAllParameterTemplateQuery, PaginatedList<ParameterTemplateDto>>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public GetAllParameterTemplateQueryHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<PaginatedList<ParameterTemplateDto>> Handle(GetAllParameterTemplateQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateService.GetAllParameterTemplateAsync(request);
    }
}

