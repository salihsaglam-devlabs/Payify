using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
public class DeleteParameterGroupCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteParameterGroupCommandHandler : IRequestHandler<DeleteParameterGroupCommand>
{
    private readonly IParameterGroupService _parameterGroupService;

    public DeleteParameterGroupCommandHandler(IParameterGroupService parameterGroupService)
    {
        _parameterGroupService = parameterGroupService;
    }
    public async Task<Unit> Handle(DeleteParameterGroupCommand request, CancellationToken cancellationToken)
    {
        await _parameterGroupService.DeleteAsync(request);

        return Unit.Value;
    }
}
