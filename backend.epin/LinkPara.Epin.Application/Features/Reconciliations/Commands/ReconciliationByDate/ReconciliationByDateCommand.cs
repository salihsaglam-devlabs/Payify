using LinkPara.Epin.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Epin.Application.Features.Reconciliations.Commands.ReconciliationByDate;

public class ReconciliationByDateCommand : IRequest
{
    public DateTime ReconciliationDate { get; set; }
}

public class ReconciliationByDateCommandHandler : IRequestHandler<ReconciliationByDateCommand>
{
    private readonly IReconciliationService _reconcilationService;

    public ReconciliationByDateCommandHandler(IReconciliationService reconcilationService)
    {
        _reconcilationService = reconcilationService;
    }

    public async Task<Unit> Handle(ReconciliationByDateCommand request, CancellationToken cancellationToken)
    {
        await _reconcilationService.ReconciliationAsync(request.ReconciliationDate);
        return Unit.Value;
    }
}
