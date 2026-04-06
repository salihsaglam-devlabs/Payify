using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
public class SaveParameterTemplateValueCommand : IRequest, IMapFrom<ParameterTemplateValue>
{
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
    public string TemplateValue { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
}

public class SaveParameterTemplateValueCommandHandler : IRequestHandler<SaveParameterTemplateValueCommand>
{
    private readonly IParameterTemplateValueService _parameterTemplateValueService;

    public SaveParameterTemplateValueCommandHandler(IParameterTemplateValueService parameterTemplateValueService)
    {
        _parameterTemplateValueService = parameterTemplateValueService;
    }
    public async Task<Unit> Handle(SaveParameterTemplateValueCommand request, CancellationToken cancellationToken)
    {
        await _parameterTemplateValueService.SaveAsync(request);

        return Unit.Value;
    }
}