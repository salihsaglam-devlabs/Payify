using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetPhysicalPosById;

public class GetPhysicalPosByIdQuery : IRequest<PhysicalPosDto>
{
    public Guid Id { get; set; }
}

public class GetPhysicalPosByIdQueryHandler : IRequestHandler<GetPhysicalPosByIdQuery, PhysicalPosDto>
{
    private readonly IPhysicalPosService _physicalPosService;

    public GetPhysicalPosByIdQueryHandler(IPhysicalPosService physicalPosService)
    {
        _physicalPosService = physicalPosService;
    }

    public async Task<PhysicalPosDto> Handle(GetPhysicalPosByIdQuery request, CancellationToken cancellationToken)
    {
        return await _physicalPosService.GetByIdAsync(request.Id);
    }
}
