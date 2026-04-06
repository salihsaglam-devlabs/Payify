using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetPhysicalPosEndOfDayDetailById;

public class GetPhysicalPosEndOfDayDetailByIdQuery : IRequest<PhysicalPosEndOfDayDetailResponse>
{
    public Guid Id { get; set; }
}

public class GetPhysicalPosEndOfDayDetailByIdQueryHandler : IRequestHandler<GetPhysicalPosEndOfDayDetailByIdQuery, PhysicalPosEndOfDayDetailResponse>
{
    private readonly IEndOfDayService _endOfDayService;
    private readonly ILogger<GetPhysicalPosEndOfDayDetailByIdQuery> _logger;

    public GetPhysicalPosEndOfDayDetailByIdQueryHandler(
        ILogger<GetPhysicalPosEndOfDayDetailByIdQuery> logger, IEndOfDayService endOfDayService)
    {
        _logger = logger;
        _endOfDayService = endOfDayService;
    }

    public async Task<PhysicalPosEndOfDayDetailResponse> Handle(GetPhysicalPosEndOfDayDetailByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _endOfDayService.GetDetailsByIdAsync(request.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetPhysicalPosEndOfDayDetailById Failed: {exception}");
            throw;
        }
        
    }
}
