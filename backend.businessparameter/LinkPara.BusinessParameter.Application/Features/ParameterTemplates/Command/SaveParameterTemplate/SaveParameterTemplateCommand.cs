using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
public class SaveParameterTemplateCommand : IRequest, IMapFrom<ParameterTemplate>
{
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
    public ParameterDataType DataType { get; set; }
    public int DataLength { get; set; }
    public string Explanation { get; set; }
}

public class SaveParameterTemplateCommandHandler : IRequestHandler<SaveParameterTemplateCommand>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public SaveParameterTemplateCommandHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<Unit> Handle(SaveParameterTemplateCommand request, CancellationToken cancellationToken)
    {
        await _parameterTemplateService.SaveAsync(request);

        return Unit.Value;
    }
}
