using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.DownloadPhysicalPosEndOfDayDetailById;

public class DownloadPhysicalPosEndOfDayDetailByIdQuery : IRequest<IActionResult>
{
    public Guid Id { get; set; }
}

public class DownloadPhysicalPosEndOfDayDetailByIdQueryHandler : IRequestHandler<DownloadPhysicalPosEndOfDayDetailByIdQuery,IActionResult>
{
    private readonly IEndOfDayService _endOfDayService;
    private readonly ILogger<DownloadPhysicalPosEndOfDayDetailByIdQuery> _logger;

    public DownloadPhysicalPosEndOfDayDetailByIdQueryHandler(IEndOfDayService endOfDayService, ILogger<DownloadPhysicalPosEndOfDayDetailByIdQuery> logger)
    {
        _endOfDayService = endOfDayService;
        _logger = logger;
    }
    
    public async Task<IActionResult> Handle(DownloadPhysicalPosEndOfDayDetailByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _endOfDayService.DownloadEndOfDayDetailByIdAsync(request.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"DownloadMerchantStatementQuery Failed: {exception}");
            throw;
        }
    }
}
