using LinkPara.Emoney.Application.Commons.Models.ReceiptModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Receipt;
using LinkPara.HttpProviders.Receipt.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class SaveReceiptConsumer : IConsumer<SaveReceiptRequest>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<SaveReceiptConsumer> _logger;
    private readonly IReceiptService _receiptService;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<WithdrawRequest> _withdrawRequestRepository;
    public SaveReceiptConsumer
        (IServiceScopeFactory scopeFactory,
        IVaultClient vaultClient,
        ILogger<SaveReceiptConsumer> logger,
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

    public async Task Consume(ConsumeContext<SaveReceiptRequest> context)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();


            var strategy = new NoRetryExecutionStrategy(dbContext);
            await strategy.ExecuteAsync(async () =>
            {

                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var mainTxnId = context.Message.TransactionId;

                    var relatedTransactions = await dbContext.Transaction
                       .Where(s => s.RelatedTransactionId == mainTxnId)
                       .ToListAsync();

                    var receiptNumber = await CallCreateReceipt(mainTxnId, relatedTransactions);

                    await dbContext.Transaction
                        .Where(t => t.Id == mainTxnId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(t => t.ReceiptNumber, receiptNumber)
                        );

                    if (relatedTransactions != null && relatedTransactions.Count > 0)
                    {
                        await dbContext.Transaction
                          .Where(t => t.RelatedTransactionId == mainTxnId)
                          .ExecuteUpdateAsync(setters => setters
                              .SetProperty(t => t.ReceiptNumber, receiptNumber)
                          );
                    }

                    await transactionScope.CommitAsync();
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
            _logger.LogError($"SaveReceiptConsumer Error: {ex}");
        }

    }

    private async Task<string> CallCreateReceipt(Guid txnId, List<Transaction> relatedTxnList)
    {
        var transaction = await _transactionRepository.GetAll(q => q.Currency)
                            .Include(x => x.Wallet)
                            .SingleOrDefaultAsync(x => x.Id == txnId);

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

        var totalAmount = transaction.TransactionType == TransactionType.Deposit
               ? transaction.Amount - (tax + commission)
               : transaction.Amount + (tax + commission);

        var receiptDetail = new
        {
            ReturnedTransactionId = transaction.ReturnedTransactionId,
            SenderWalletNo = counterWalletNumber,
            SenderWalletName = counterWalletName,
            ReceiverWalletNo = transaction?.Wallet?.WalletNumber,
            ReceiverWalletName = transaction.ReceiverName,
            ReceiverIban = receiverIban
        };

        ReceiptDto receipt = new ReceiptDto()
        {
            RefTransactionId = transaction.Id,
            Module = "Emoney",
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType.ToString(),
            TransactionDirection = transaction.TransactionDirection.ToString(),
            PaymentMethod = transaction.PaymentMethod.ToString(),
            Amount = transaction.Amount,
            CommissionAmount = commission,
            TaxAmount = tax,
            TotalAmount = totalAmount,
            CurrencyCode = transaction.CurrencyCode,
            Description = transaction.Description,
            DetailInfo = JsonConvert.SerializeObject(receiptDetail)
        };

        CreateReceiptRequest receiptRequest = new CreateReceiptRequest() { ReceiptInfo = receipt };
        var receiptResult = await _receiptService.CreateReceiptAsync(receiptRequest);

        return receiptResult != null && receiptResult.Success ? receiptResult.ReceiptNumber : String.Empty;
    }
}