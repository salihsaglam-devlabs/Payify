using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class MoneyTransferFailedConsumer : IConsumer<TransferFailed>
{
    private readonly ILogger<MoneyTransferCompletedConsumer> _logger;
    private readonly IGenericRepository<PostingBalance> _repository;
    private readonly IBus _bus;

    public MoneyTransferFailedConsumer(
        ILogger<MoneyTransferCompletedConsumer> logger,
        IGenericRepository<PostingBalance> repository, IBus bus)
    {
        _logger = logger;
        _repository = repository;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<TransferFailed> context)
    {
        var transferResponse = context.Message;

        try
        {
            var balances = await _repository
                .GetAll()
                .Include(s => s.Merchant)
                .Where(w => w.TransactionSourceId == transferResponse.TransactionSourceReferenceId)
                .ToListAsync();

            if (!balances.Any())
            {
                _logger.LogCritical($"MoneyTransferFailedButReferenceNotFound: {transferResponse.TransactionSourceReferenceId}");

                return;
            }

            balances.ForEach(b =>
            {
                b.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentError;
                b.BatchStatus = BatchStatus.MoneyTransferError;
                b.ProcessingId = null;
            });

            await _repository.UpdateRangeAsync(balances);

            var balance = balances.First();
            await _bus.Publish(new PostingMoneyTransferFailed
            {
                MerchantNumber = balance.Merchant.Number,
                MerchantName = balance.Merchant.Name,
                PaymentDate = balance.PaymentDate,
                Iban = balance.Iban,
                WalletNumber = balance.WalletNumber,
                MoneyTransferBankCode = balance.MoneyTransferBankCode,
                MoneyTransferBankName = balance.MoneyTransferBankName,
                TotalPayingAmount = balances.Sum(s => s.TotalPayingAmount),
                MoneyTransferReferenceId = balance.MoneyTransferReferenceId,
            }, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"ErrorConsumingMoneyTransferFailedEvent: {exception}");
        }
    }
}