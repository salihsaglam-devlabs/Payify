using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetAllParameterTemplate;

public class GetAllParameterTemplateByGroupCodeQuery : IRequest<List<ParameterTemplateDto>>
{
    public string GroupCode { get; set; }
}

public class GetAllParameterTemplateByGroupCodeQueryHandler : IRequestHandler<GetAllParameterTemplateByGroupCodeQuery, List<ParameterTemplateDto>>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public GetAllParameterTemplateByGroupCodeQueryHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<List<ParameterTemplateDto>> Handle(GetAllParameterTemplateByGroupCodeQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateService.GetAllParameterTemplateByGroupCodeAsync(request.GroupCode);
    }
}
