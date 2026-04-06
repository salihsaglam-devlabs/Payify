using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
public class UpdateParameterGroupCommand : IRequest<ParameterGroupDto>, IMapFrom<ParameterGroup>
{
    [Audit]
    public Guid Id { get; set; }
    [Audit]
    public string GroupCode { get; set; }
    public string Explanation { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
public class UpdateParameterGroupHandler : IRequestHandler<UpdateParameterGroupCommand, ParameterGroupDto>
{
    private readonly IParameterGroupService _parameterGroupService;
    public UpdateParameterGroupHandler(IParameterGroupService parameterGroupService)
    {
        _parameterGroupService = parameterGroupService;
    }
    public async Task<ParameterGroupDto> Handle(UpdateParameterGroupCommand request, CancellationToken cancellationToken)
    {
        return await _parameterGroupService.UpdateAsync(request);
    }
}
