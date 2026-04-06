using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Commands.ApproveBulkTransfer;

public class ActionBulkTransferCommand : IRequest
{
    public Guid BulkTransferId { get; set; }
    public bool IsApprove { get; set; }
}

public class ActionBulkTransferCommandHandler : IRequestHandler<ActionBulkTransferCommand>
{
    private readonly IBulkTransferService _bulkTransferService;

    public ActionBulkTransferCommandHandler(IBulkTransferService bulkTransferService)
    {
        _bulkTransferService = bulkTransferService;
    }

    public async Task<Unit> Handle(ActionBulkTransferCommand request, CancellationToken cancellationToken)
    {
        await _bulkTransferService.ActionBulkTransferAsync(request, cancellationToken);
        return Unit.Value;
    }
}