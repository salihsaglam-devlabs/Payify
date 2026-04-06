using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Cashback.Models;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Models;

namespace LinkPara.Emoney.Infrastructure.Services;

public class CashbackService : ICashbackService
{
    private readonly IBus _bus;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public CashbackService(IBus bus, IGenericRepository<Transaction> transactionRepository)
    {
        _bus = bus;
        _transactionRepository = transactionRepository;
    }

    public async Task SendCashbackQueueAsync(SendCashbackQueueRequest request)
    {
        var transaction = await _transactionRepository.GetAll(q => q.Currency)
                           .Include(x => x.Wallet)
                           .ThenInclude(a => a.Account)
                           .AsNoTracking()
                           .SingleOrDefaultAsync(x => x.Id == request.TransactionId);



        var checkTransactionRequest = new CheckTransactionForCashbackRequest()
        {
            TransactionInfo = new CashbackTransactionDto()
            {
                OriginalTransactionId = request.TransactionId,
                TransactionType = transaction.TransactionType.ToString(),
                PaymentMethod = transaction.PaymentMethod.ToString(),
                TransactionStatus = transaction.TransactionStatus.ToString(),
                TransactionDirection = transaction.TransactionDirection.ToString(),
                Amount = transaction.Amount,
                CurrencyCode = transaction.CurrencyCode,
                TransactionDate = transaction.TransactionDate,
                UserId = Guid.Parse(transaction.CreatedBy),
                Name = transaction.Wallet.Account.Name,
                WalletId = transaction.WalletId,
                WalletNo = transaction.Wallet.WalletNumber,
                CorporateWalletNumber = request.CorporateWalletNumber,
                AccountKycLevel = transaction.Wallet.Account.AccountKycLevel.ToString(),
                ConversationId = request.ConversationId,
                CorporateAccountName = request.CorporateAccountName
            }
        };

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Cashback.CheckTransactionForCashbackRequest"));
        await endpoint.Send(checkTransactionRequest, tokenSource.Token);
    }
}




