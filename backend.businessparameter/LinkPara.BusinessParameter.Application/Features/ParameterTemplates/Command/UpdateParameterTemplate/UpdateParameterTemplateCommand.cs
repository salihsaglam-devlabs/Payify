using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
public class UpdateParameterTemplateCommand : IRequest<ParameterTemplateDto>, IMapFrom<ParameterTemplate>
{
    [Audit]
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    [Audit]
    public string TemplateCode { get; set; }
    public ParameterDataType DataType { get; set; }
    public int DataLength { get; set; }
    public string Explanation { get; set; }
}

public class UpdateParameterTemplateHandler : IRequestHandler<UpdateParameterTemplateCommand, ParameterTemplateDto>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public UpdateParameterTemplateHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<ParameterTemplateDto> Handle(UpdateParameterTemplateCommand request, CancellationToken cancellationToken)
    {
        return await _parameterTemplateService.UpdateAsync(request);
    }
}
