using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
public class GetParameterTemplateByIdQuery : IRequest<ParameterTemplateDto>
{
    public Guid Id { get; set; }
}

public class GetParameterTemplateByIdQueryHandler : IRequestHandler<GetParameterTemplateByIdQuery, ParameterTemplateDto>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public GetParameterTemplateByIdQueryHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<ParameterTemplateDto> Handle(GetParameterTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateService.GetByIdAsync(request);
    }
}
