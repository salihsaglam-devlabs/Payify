using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Command.SavePhysicalPos;

public class SavePhysicalPosCommand : IRequest
{
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public VposType VposType { get; set; }
    public string PfMainMerchantId { get; set; }
}

public class SavePhysicalPosCommandHandler : IRequestHandler<SavePhysicalPosCommand>
{
    private readonly IPhysicalPosService _physicalPosService;

    public SavePhysicalPosCommandHandler(IPhysicalPosService physicalPosService)
    {
        _physicalPosService = physicalPosService;
    }

    public async Task<Unit> Handle(SavePhysicalPosCommand request, CancellationToken cancellationToken)
    {
        await _physicalPosService.SaveAsync(request);

        return Unit.Value;
    }
}
