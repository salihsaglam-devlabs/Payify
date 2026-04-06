using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Emoney;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class MoneyTransferReturnedConsumer : IConsumer<ReturnedWithdrawTransaction>
{
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly ILogger<MoneyTransferReturnedConsumer> _logger;

    public MoneyTransferReturnedConsumer(ILogger<MoneyTransferReturnedConsumer> logger, IGenericRepository<PostingBalance> postingBalanceRepository)
    {
        _logger = logger;
        _postingBalanceRepository = postingBalanceRepository;
    }

    public async Task Consume(ConsumeContext<ReturnedWithdrawTransaction> context)
    {
        var transferResponse = context.Message;
        
        try
        {
            var balances = await _postingBalanceRepository
                .GetAll()
                .Where(w => w.MoneyTransferReferenceId == transferResponse.MoneyTransferReferenceId)
                .ToListAsync();

            if (!balances.Any())
            {
                _logger.LogCritical($"MoneyTransferReturnedButReferenceNotFound: {transferResponse.MoneyTransferReferenceId}");

                return;
            }

            balances.ForEach(b =>
            {
                b.BatchStatus = BatchStatus.MoneyTransferError;
                b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentReturned;
                b.ProcessingId = null;
            });

            await _postingBalanceRepository.UpdateRangeAsync(balances);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorConsumingMoneyTransferReturnedEvent: {exception}");
        }
    }
}