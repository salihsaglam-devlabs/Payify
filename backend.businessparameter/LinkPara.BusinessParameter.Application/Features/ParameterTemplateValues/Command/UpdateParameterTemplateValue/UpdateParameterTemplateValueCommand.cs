using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.UpdateParameterTemplateValue;
public class UpdateParameterTemplateValueCommand : IRequest<ParameterTemplateValueDto>,IMapFrom<ParameterTemplateValue>
{
    [Audit]
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    [Audit]
    public string TemplateCode { get; set; }
    public string TemplateValue { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
}

public class UpdateParameterTemplateValueHandler : IRequestHandler<UpdateParameterTemplateValueCommand, ParameterTemplateValueDto>
{
    private readonly IParameterTemplateValueService _parameterTemplateValueService;

    public UpdateParameterTemplateValueHandler(IParameterTemplateValueService parameterTemplateValueService)
    {
        _parameterTemplateValueService = parameterTemplateValueService;
    }
    public async Task<ParameterTemplateValueDto> Handle(UpdateParameterTemplateValueCommand request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateValueService.UpdateAsync(request);
    }
}
