using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetAllParameterTemplateValue;

public class GetAllParameterTemplateValueByGroupCodeQuery : IRequest<List<ParameterTemplateValueDto>>
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
}

public class GetAllParameterTemplateValueByGroupCodeQueryHandler : IRequestHandler<GetAllParameterTemplateValueByGroupCodeQuery, List<ParameterTemplateValueDto>>
{
    private readonly IParameterTemplateValueService _parameterTemplateValue;

    public GetAllParameterTemplateValueByGroupCodeQueryHandler(IParameterTemplateValueService parameterTemplateValue)
    {
        _parameterTemplateValue = parameterTemplateValue;
    }
    public async Task<List<ParameterTemplateValueDto>> Handle(GetAllParameterTemplateValueByGroupCodeQuery request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateValue.GetAllParameterTemplateValuesByGroupCodeAsync(request.GroupCode, request.ParameterCode);
    }
}
