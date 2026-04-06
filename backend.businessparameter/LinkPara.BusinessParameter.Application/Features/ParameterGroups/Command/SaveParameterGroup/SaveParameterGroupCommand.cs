using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
public class SaveParameterGroupCommand : IRequest, IMapFrom<ParameterGroup>
{
    public string GroupCode { get; set; }
    public string Explanation { get; set; }
}

public class SaveParameterGroupCommandHandler : IRequestHandler<SaveParameterGroupCommand>
{
    private readonly IParameterGroupService _parameterGroupService;

    public SaveParameterGroupCommandHandler(IParameterGroupService parameterGroupService)
    {
        _parameterGroupService = parameterGroupService;
    }
    public async Task<Unit> Handle(SaveParameterGroupCommand request, CancellationToken cancellationToken)
    {
        await _parameterGroupService.SaveAsync(request);

        return Unit.Value;
    }
}