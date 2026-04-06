using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;

public class UpdateParameterCommand : IRequest<ParameterDto>, IMapFrom<Parameter>
{
    [Audit]
    public Guid Id { get; set; }
    [Audit]
    public string ParameterValue { get; set; }
    public List<UpdateParemeterTemplateValueDto> ParameterTemplateValueList { get; set; }
}

public class UpdateParameterHandler : IRequestHandler<UpdateParameterCommand, ParameterDto>
{
    private readonly IParameterService _parameterService;

    public UpdateParameterHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<ParameterDto> Handle(UpdateParameterCommand request, CancellationToken cancellationToken)
    {
        return await _parameterService.UpdateAsync(request);
    }
}