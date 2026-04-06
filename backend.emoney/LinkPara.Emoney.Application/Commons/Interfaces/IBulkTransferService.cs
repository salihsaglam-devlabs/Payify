using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using LinkPara.Emoney.Application.Features.BulkTransfers.Commands.ApproveBulkTransfer;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IBulkTransferService
{
    Task ActionBulkTransferAsync(ActionBulkTransferCommand request, CancellationToken cancellationToken);
    Task BulkTransferAsync(Guid bulkTransferId, Guid userId, CancellationToken cancellationToken);
    Task CheckWithdrawBulkTransferAsync(CheckWithdrawBulkTransferRequest request);
    Task SendWithdrawBulkTransferAsync(CheckWithdrawBulkTransferRequest request);
}
