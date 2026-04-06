using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using LinkPara.BusinessParameter.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;

public class SaveParameterCommand : IRequest, IMapFrom<Parameter>
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
    public List<SaveParameterTemplateValueDto> ParameterTemplateValueList { get; set; }
}

public class SaveParameterCommandHandler : IRequestHandler<SaveParameterCommand>
{
    private readonly IParameterService _parameterService;

    public SaveParameterCommandHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<Unit> Handle(SaveParameterCommand request, CancellationToken cancellationToken)
    {
        await _parameterService.SaveAsync(request);

        return Unit.Value;
    }
}