using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class BulkTransferConsumer : IConsumer<BulkTransferRequest>
{
    private readonly IBulkTransferService _bulkTransferService;

    public BulkTransferConsumer(IBulkTransferService bulkTransferService)
    {
        _bulkTransferService = bulkTransferService;
    }
    public async Task Consume(ConsumeContext<BulkTransferRequest> context)
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await _bulkTransferService.BulkTransferAsync(context.Message.BulkTransferId,context.Message.UserId, cancellationToken.Token);
    }
}
