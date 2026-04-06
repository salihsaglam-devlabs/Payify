using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class MoneyTransferSavedConsumer : IConsumer<SaveTransferResponse>
{
    private readonly ILogger<MoneyTransferSavedConsumer> _logger;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;

    public MoneyTransferSavedConsumer(ILogger<MoneyTransferSavedConsumer> logger, IGenericRepository<PostingBalance> postingBalanceRepository)
    {
        _logger = logger;
        _postingBalanceRepository = postingBalanceRepository;
    }

    public async Task Consume(ConsumeContext<SaveTransferResponse> context)
    {
        var transferResponse = context.Message;

        var balances = await _postingBalanceRepository
            .GetAll()
            .Where(b => b.TransactionSourceId == transferResponse.TransactionSourceId)
            .ToListAsync();

        if(!balances.Any())
        {
            _logger.LogError($"MoneyTransferRequestSavedButBalanceNotFound: {transferResponse.TransactionSourceId}");
        }

        balances.ForEach(b =>
        {
            b.MoneyTransferBankCode = transferResponse.TransferBankCode;
            b.MoneyTransferBankName = transferResponse.TransferBankName;
            b.MoneyTransferStatus = b.MoneyTransferStatus == PostingMoneyTransferStatus.PaymentInitiated
                ? PostingMoneyTransferStatus.PaymentDelivered
                : b.MoneyTransferStatus;
            b.MoneyTransferReferenceId = transferResponse.ReferenceId;
            b.ProcessingId = null;
        });

        await _postingBalanceRepository.UpdateRangeAsync(balances);
    }
}