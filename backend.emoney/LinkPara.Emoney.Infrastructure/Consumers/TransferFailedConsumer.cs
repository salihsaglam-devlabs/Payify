using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class TransferFailedConsumer : IConsumer<TransferFailed>
{
    private readonly ILogger<TransferFailedConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IContextProvider _contextProvider;
    private readonly ILimitService _limitService;
    private readonly IAccountActivityService _accountActivityService;
    private readonly IBulkTransferService _bulkTransferService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;

    public TransferFailedConsumer(
        ILogger<TransferFailedConsumer> logger,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory,
        IContextProvider contextProvider,
        ILimitService limitService,
        IAccountActivityService accountActivityService,
        IBulkTransferService bulkTransferService,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
        _contextProvider = contextProvider;
        _limitService = limitService;
        _accountActivityService = accountActivityService;
        _bulkTransferService = bulkTransferService;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
    }

    public async Task Consume(ConsumeContext<TransferFailed> context)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var request = await dbContext.WithdrawRequest
                    .SingleOrDefaultAsync(s =>
                        s.Id == context.Message.TransactionSourceReferenceId &&
                        s.RecordStatus == RecordStatus.Active &&
                        s.WithdrawStatus == WithdrawStatus.Delivered);

        if (request is null)
        {
            _logger.LogError($"WithdrawRequestNotFound! TransactionSourceReferenceId : {context.Message.TransactionSourceReferenceId}");

            return;
        }

        try
        {
            var depositTransactionId = Guid.Empty;

            var strategy = new NoRetryExecutionStrategy(dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var transaction = await dbContext.Transaction.SingleOrDefaultAsync(
                            s =>
                            s.Id == request.InternalTransactionId &&
                            s.IsReturned == false
                        );

                    await UpdateCurrentTransactionsAsync(context, transaction, request, dbContext);

                    var depositTransaction = await ReturnToWalletAsync(request, transaction, dbContext);

                    await dbContext.SaveChangesAsync();

                    await transactionScope.CommitAsync();

                    depositTransactionId = depositTransaction.Id;

                    await SendBulkTransferQueueAsync(transaction);
                }
                catch (Exception)
                {
                    await transactionScope.RollbackAsync();

                    throw;
                }
            });

            await _saveReceiptService.SendReceiptQueueAsync(depositTransactionId);
        }
        catch (Exception exception)
        {
            _logger.LogError($"TransferFailed Consumer Error {exception}", exception);
        }
    }

    private async Task UpdateCurrentTransactionsAsync(ConsumeContext<TransferFailed> context, Transaction transaction,
        WithdrawRequest request, EmoneyDbContext dbContext)
    {
        request.WithdrawStatus = WithdrawStatus.Failed;
        request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
        request.UpdateDate = DateTime.Now;
        request.MoneyTransferPaymentDate = context.Message.TransferDate;
        request.MoneyTransferReferenceId = context.Message.MoneyTransferPaymentId;
        request.MoneyTransferStatus = context.Message.MoneyTransferStatus;
        dbContext.Update(request);

        transaction.TransactionStatus = TransactionStatus.Completed;
        transaction.ExternalTransactionDate = context.Message.TransferDate;
        transaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
        transaction.UpdateDate = DateTime.Now;
        transaction.IsReturned = true;

        dbContext.Update(transaction);

        var pricingTransactions = dbContext.Transaction.Where(s => s.RelatedTransactionId == transaction.Id);
        foreach (var pricingTransaction in pricingTransactions)
        {
            pricingTransaction.TransactionStatus = TransactionStatus.Completed;
            pricingTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            pricingTransaction.UpdateDate = DateTime.Now;
            pricingTransaction.IsReturned = true;

            dbContext.Update(pricingTransaction);
        }

        await Task.CompletedTask;
    }

    private async Task<Transaction> ReturnToWalletAsync(WithdrawRequest request, Transaction transaction, EmoneyDbContext dbContext)
    {
        var transactionList = new List<Transaction>();

        var pricingTransactions = dbContext.Transaction
            .Where(s => s.RelatedTransactionId == transaction.Id);

        foreach (var item in pricingTransactions)
        {
            transactionList.Add(item);
        }

        var wallet = await GetWalletWithLockAsync(dbContext, transaction.WalletId);

        var receiverAccount = await dbContext.Account.FirstOrDefaultAsync(s => s.Id == wallet.AccountId);

        var mainTransaction = PopulateDepositTransaction(wallet, transaction, receiverAccount);
        await dbContext.AddAsync(mainTransaction);

        wallet.CurrentBalanceCash += mainTransaction.Amount;

        foreach (var item in transactionList)
        {
            var newTransaction = PopulateDepositTransaction(wallet, item, receiverAccount);
            newTransaction.RelatedTransactionId = mainTransaction.Id;

            await dbContext.AddAsync(newTransaction);

            wallet.CurrentBalanceCash += item.Amount;
        }

        wallet.LastActivityDate = DateTime.Now;
        wallet.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

        dbContext.Update(wallet);

        var level = await dbContext.AccountCurrentLevel
            .FirstOrDefaultAsync(s =>
                s.AccountId == wallet.AccountId && s.CurrencyCode == wallet.CurrencyCode);

        if (level is not null)
        {
            var isDailyDistinctIban = false;
            var isMonthlyDistinctIban = false;
            if (!request.IsReceiverIbanOwned)
            {
                isDailyDistinctIban =
                    await _accountActivityService.IsWithdrawIbanDistinctAsync(
                        wallet.AccountId, TimeInterval.Daily, request.ReceiverIbanNumber);
                isMonthlyDistinctIban =
                    await _accountActivityService.IsWithdrawIbanDistinctAsync(
                        wallet.AccountId, TimeInterval.Monthly, request.ReceiverIbanNumber);
            }

            await _limitService.DecreaseUsageAsync(new AccountLimitUpdateRequest
            {
                Amount = transaction.Amount,
                CurrencyCode = wallet.CurrencyCode,
                LimitOperationType = LimitOperationType.Withdrawal,
                AccountId = wallet.AccountId,
                WalletType = wallet.WalletType,
                IsOwnIban = request.IsReceiverIbanOwned,
                IsDailyDistinctIban = isDailyDistinctIban,
                IsMonthlyDistinctIban = isMonthlyDistinctIban
            }, level);

            dbContext.Update(level);
        }

        return mainTransaction;
    }

    private Transaction PopulateDepositTransaction(Wallet wallet, Transaction item, Account receiverAccount)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Return,
            TransactionStatus = TransactionStatus.Completed,
            Tag = item.Tag,
            TagTitle = TransactionType.Return.ToString(),
            Amount = item.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = item.Description,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            CurrentBalance = wallet.AvailableBalance + item.Amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = item.PaymentMethod,
            RecordStatus = RecordStatus.Active,
            SenderBankCode = 0,
            SenderName = string.Empty,
            ReceiverName = receiverAccount != null ? receiverAccount.Name : string.Empty,
            ReturnedTransactionId = item.Id,
            Channel = _contextProvider.CurrentContext?.Channel
        };
    }

    private async Task<Wallet> GetWalletWithLockAsync(EmoneyDbContext dbContext, Guid id)
    {
        var databaseProvider = await _databaseProviderService.GetProviderAsync();

        switch (databaseProvider)
        {
            case "MsSql":
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM Core.Wallet WITH(ROWLOCK, UPDLOCK) " +
                                    "WHERE Id = {0} " +
                                    "AND RecordStatus = 'Active'", id)
                        .FirstOrDefaultAsync();
                }
            default:
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM core.wallet " +
                                    "WHERE id = {0} " +
                                    "AND record_status = 'Active' FOR UPDATE", id)
                        .FirstOrDefaultAsync();
                }
        }
    }

    private async Task SendBulkTransferQueueAsync(Transaction transaction)
    {
        var request = new CheckWithdrawBulkTransferRequest
        {
            TransactionId = transaction.Id,
            TransactionResult = TransactionResult.Failed
        };
        await _bulkTransferService.SendWithdrawBulkTransferAsync(request);
    }
}
