using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Command.SingleManualReconciliation;

public class SingleManualReconciliationCommand : IRequest
{
    public Guid MerchantTransactionId { get; set; }
}

public class SingleManualReconciliationCommandHandler : IRequestHandler<SingleManualReconciliationCommand>
{
    private readonly IEndOfDayService _endOfDayService;
    private readonly ILogger<SingleManualReconciliationCommand> _logger;

    public SingleManualReconciliationCommandHandler(IEndOfDayService endOfDayService, ILogger<SingleManualReconciliationCommand> logger)
    {
        _endOfDayService = endOfDayService;
        _logger = logger;
    }
    public async Task<Unit> Handle(SingleManualReconciliationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _endOfDayService.SingleManualReconciliationAsync(request.MerchantTransactionId);
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"SingleManualReconciliation Failed: {exception}");
            throw;
        }
    }
}