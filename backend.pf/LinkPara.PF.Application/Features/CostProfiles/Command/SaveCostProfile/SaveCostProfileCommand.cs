using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;

public class SaveCostProfileCommand : IRequest
{
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public PosType PosType { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public Guid? VposId { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}

public class SaveCostProfileCommandHandler : IRequestHandler<SaveCostProfileCommand>
{
    private readonly ICostProfileService _costProfileService;

    public SaveCostProfileCommandHandler(ICostProfileService costProfileService)
    {
        _costProfileService = costProfileService;
    }
    public async Task<Unit> Handle(SaveCostProfileCommand request, CancellationToken cancellationToken)
    {
        await _costProfileService.SaveAsync(request);

        return Unit.Value;
    }
}
