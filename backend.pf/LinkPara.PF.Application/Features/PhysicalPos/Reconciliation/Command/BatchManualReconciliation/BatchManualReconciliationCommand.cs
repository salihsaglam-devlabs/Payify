using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.BatchManualReconciliation;

public class BatchManualReconciliationCommand : IRequest
{
    public Guid EndOfDayId { get; set; }
}

public class BatchManualReconciliationCommandHandler : IRequestHandler<BatchManualReconciliationCommand>
{
    private readonly IEndOfDayService _endOfDayService;
    private readonly ILogger<BatchManualReconciliationCommand> _logger;

    public BatchManualReconciliationCommandHandler(IEndOfDayService endOfDayService, ILogger<BatchManualReconciliationCommand> logger)
    {
        _endOfDayService = endOfDayService;
        _logger = logger;
    }
    public async Task<Unit> Handle(BatchManualReconciliationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _endOfDayService.BatchManualReconciliationAsync(request.EndOfDayId);
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"BatchManualReconciliation Failed: {exception}");
            throw;
        }
    }
}