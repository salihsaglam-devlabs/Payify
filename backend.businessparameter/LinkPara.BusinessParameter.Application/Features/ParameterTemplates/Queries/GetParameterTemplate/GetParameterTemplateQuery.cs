using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplate;

public class GetParameterTemplateQuery : IRequest<ParameterTemplateDto>
{
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
}

public class GetParameterTemplateQueryHandler : IRequestHandler<GetParameterTemplateQuery, ParameterTemplateDto>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public GetParameterTemplateQueryHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }

    public async Task<ParameterTemplateDto> Handle(GetParameterTemplateQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateService.GetParameterTemplateAsync(request.GroupCode, request.TemplateCode);
    }
}
