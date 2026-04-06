using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class CheckWithdrawBulkTransferConsumer : IConsumer<CheckWithdrawBulkTransferRequest>
{
    private readonly IBulkTransferService _bulkTransferService;

    public CheckWithdrawBulkTransferConsumer(IBulkTransferService bulkTransferService)
    {
        _bulkTransferService = bulkTransferService;
    }

    public async Task Consume(ConsumeContext<CheckWithdrawBulkTransferRequest> context)
    {
        await _bulkTransferService.CheckWithdrawBulkTransferAsync(context.Message);
    }
}
