using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetAllPhysicalPos;

public class GetAllPhysicalPosQuery : SearchQueryParams, IRequest<PaginatedList<PhysicalPosDto>>
{
    public int? BankCode { get; set; }
    public VposStatus? VposStatus { get; set; }
    public VposType? VposType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetAllPhysicalPosQueryHandler : IRequestHandler<GetAllPhysicalPosQuery, PaginatedList<PhysicalPosDto>>
{
    private readonly IPhysicalPosService _physicalPosService;

    public GetAllPhysicalPosQueryHandler(IPhysicalPosService physicalPosService)
    {
        _physicalPosService = physicalPosService;
    }
    public async Task<PaginatedList<PhysicalPosDto>> Handle(GetAllPhysicalPosQuery request, CancellationToken cancellationToken)
    {
        return await _physicalPosService.GetAllAsync(request);
    }
}
