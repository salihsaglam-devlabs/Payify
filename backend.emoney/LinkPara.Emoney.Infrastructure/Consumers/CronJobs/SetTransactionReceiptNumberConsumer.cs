using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Receipt;
using LinkPara.HttpProviders.Receipt.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class SetTransactionReceiptNumberConsumer : IConsumer<SetTransactionReceiptNumber>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<SetTransactionReceiptNumberConsumer> _logger;
    private readonly IReceiptService _receiptService;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<WithdrawRequest> _withdrawRequestRepository;
    public SetTransactionReceiptNumberConsumer
        (IServiceScopeFactory scopeFactory,
        IVaultClient vaultClient,
        ILogger<SetTransactionReceiptNumberConsumer> logger,
        IReceiptService receiptService,
        IGenericRepository<Transaction> transactionRepository,
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<WithdrawRequest> withdrawRequestRepository)
    {
        _scopeFactory = scopeFactory;
        _vaultClient = vaultClient;
        _logger = logger;
        _receiptService = receiptService;
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _withdrawRequestRepository = withdrawRequestRepository;
    }

    public async Task Consume(ConsumeContext<SetTransactionReceiptNumber> context)
    {
        try
        {
            var txnSettings = _vaultClient.GetSecretValue<TransactionSettings>("EmoneySecrets", "TransactionSettings");

            if (!txnSettings.ReceiptNumberAssignment)
            {
                return;
            }

            var processLimit = txnSettings.ReceiptTransactionLimit;
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();


            var strategy = new NoRetryExecutionStrategy(dbContext);
            await strategy.ExecuteAsync(async () =>
            {

                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var mainTransactions = await dbContext.Transaction
                           .FromSqlRaw("SELECT * FROM core.transaction t WHERE related_transaction_id is null " +
                           "AND receipt_number is null AND transaction_status = 'Completed' " +
                           "AND transaction_date >= '2025-01-01 00:00:00.000'" +
                           "ORDER BY create_date FOR update LIMIT {0}", processLimit)
                           .ToListAsync();

                    if (mainTransactions != null && mainTransactions.Count > 0)
                    {
                        foreach (var mainTxn in mainTransactions)
                        {
                            var relatedTransactions = await dbContext.Transaction
                               .Where(s => s.RelatedTransactionId == mainTxn.Id)
                               .ToListAsync();

                            var receiptNumber = await CallCreateReceipt(mainTxn, relatedTransactions);

                            await dbContext.Transaction
                                .Where(t => t.Id == mainTxn.Id)
                                .ExecuteUpdateAsync(setters => setters
                                    .SetProperty(t => t.ReceiptNumber, receiptNumber)
                                );

                            if (relatedTransactions != null && relatedTransactions.Count > 0)
                            {
                                await dbContext.Transaction
                                  .Where(t => t.RelatedTransactionId == mainTxn.Id)
                                  .ExecuteUpdateAsync(setters => setters
                                      .SetProperty(t => t.ReceiptNumber, receiptNumber)
                                  );
                            }
                        }

                        await transactionScope.CommitAsync();
                    }
                }
                catch (Exception)
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }
            });

        }
        catch (Exception ex)
        {
            _logger.LogError($"SetTransactionReceiptNumberConsumer Error: {ex}");
        }

    }

    private async Task<string> CallCreateReceipt(Transaction txn, List<Transaction> relatedTxnList)
    {
        var transaction = await _transactionRepository.GetAll(q => q.Currency)
                            .Include(x => x.Wallet)
                            .SingleOrDefaultAsync(x => x.Id == txn.Id);

        var counterWalletNumber = String.Empty;
        var counterWalletName = String.Empty;
        if (transaction.CounterWalletId != null)
        {
            var counterWallet = await _walletRepository.GetAll()
                .Include(x => x.Account)
                .Select(x => new
                {
                    x.Id,
                    x.WalletNumber,
                    x.Account.Name
                }).FirstOrDefaultAsync(x => x.Id == transaction.CounterWalletId);

            counterWalletNumber = counterWallet?.WalletNumber;
            counterWalletName = counterWallet?.Name;
        }

        var receiverIban = String.Empty;
        if (transaction.WithdrawRequestId is not null)
        {
            var withdrawRequest = await _withdrawRequestRepository.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.ReceiverIbanNumber
                }).FirstOrDefaultAsync(x => x.Id == transaction.WithdrawRequestId);
            receiverIban = withdrawRequest?.ReceiverIbanNumber;
        }

        var tax = Decimal.Zero;
        var commission = Decimal.Zero;

        if (relatedTxnList != null && relatedTxnList.Count > 0)
        {
            var bsmv = relatedTxnList.Select(x => new
            {
                x.Tag,
                x.Amount,
                x.TransactionType
            }).FirstOrDefault(s => s.TransactionType == TransactionType.Tax);

            var pricing = relatedTxnList.Select(x => new
            {
                x.Tag,
                x.Amount,
                x.TransactionType
            }).FirstOrDefault(s => s.TransactionType == TransactionType.Commission);

            tax = bsmv?.Amount ?? Decimal.Zero;
            commission = pricing?.Amount ?? Decimal.Zero;
        }

        var totalAmount = txn.TransactionType == TransactionType.Deposit
               ? txn.Amount - (tax + commission)
               : txn.Amount + (tax + commission);

        var receiptDetail = new { 
            ReturnedTransactionId = txn.ReturnedTransactionId,
            SenderWalletNo = counterWalletNumber,
            SenderWalletName = counterWalletName,
            ReceiverWalletNo = transaction?.Wallet?.WalletNumber,
            ReceiverWalletName = txn.ReceiverName,
            ReceiverIban = receiverIban
        };

        ReceiptDto receipt = new ReceiptDto() {
            RefTransactionId = txn.Id,
            Module = "Emoney",
            TransactionDate = txn.TransactionDate,
            TransactionType = txn.TransactionType.ToString(),
            TransactionDirection = txn.TransactionDirection.ToString(),
            PaymentMethod = txn.PaymentMethod.ToString(),
            Amount = txn.Amount,
            CommissionAmount = commission,
            TaxAmount = tax,
            TotalAmount = totalAmount,
            CurrencyCode = txn.CurrencyCode,
            Description = txn.Description,
            DetailInfo = JsonConvert.SerializeObject(receiptDetail)
        };

        CreateReceiptRequest receiptRequest = new CreateReceiptRequest() { ReceiptInfo = receipt };
        var receiptResult = await _receiptService.CreateReceiptAsync(receiptRequest);

        return receiptResult != null && receiptResult.Success ? receiptResult.ReceiptNumber : String.Empty;
    }
}