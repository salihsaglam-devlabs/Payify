using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Command.DeletePhysicalPos;

public class DeletePhysicalPosCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePhysicalPosCommandHandler : IRequestHandler<DeletePhysicalPosCommand>
{
    private readonly IPhysicalPosService _physicalPosService;

    public DeletePhysicalPosCommandHandler(IPhysicalPosService physicalPosService)
    {
        _physicalPosService = physicalPosService;
    }

    public async Task<Unit> Handle(DeletePhysicalPosCommand request, CancellationToken cancellationToken)
    {
        await _physicalPosService.DeleteAsync(request);

        return Unit.Value;
    }
}
