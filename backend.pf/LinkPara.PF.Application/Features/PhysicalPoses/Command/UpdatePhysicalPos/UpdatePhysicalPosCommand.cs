using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Command.UpdatePhysicalPos;

public class UpdatePhysicalPosCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public VposType VposType { get; set; }
    public string PfMainMerchantId { get; set; }
}

public class UpdatePhysicalPosCommandHandler : IRequestHandler<UpdatePhysicalPosCommand>
{
    private readonly IPhysicalPosService _physicalPosService;

    public UpdatePhysicalPosCommandHandler(IPhysicalPosService physicalPosService)
    {
        _physicalPosService = physicalPosService;
    }

    public async Task<Unit> Handle(UpdatePhysicalPosCommand request, CancellationToken cancellationToken)
    {
        await _physicalPosService.UpdateAsync(request);

        return Unit.Value;
    }
}
