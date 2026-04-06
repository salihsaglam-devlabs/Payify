using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValue;

public class GetParameterTemplateValueQuery : IRequest<ParameterTemplateValueDto>
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string TemplateCode { get; set; }
}

public class GetParameterTemplateValueQueryHandler : IRequestHandler<GetParameterTemplateValueQuery, ParameterTemplateValueDto>
{
    private readonly IParameterTemplateValueService _valueService;

    public GetParameterTemplateValueQueryHandler(IParameterTemplateValueService valueService)
    {
        _valueService = valueService;
    }

    public async Task<ParameterTemplateValueDto> Handle(GetParameterTemplateValueQuery request, CancellationToken cancellationToken)
    {
        return await _valueService.GetParameterTemplateValueAsync(request.GroupCode, request.ParameterCode, request.TemplateCode);
    }
}
