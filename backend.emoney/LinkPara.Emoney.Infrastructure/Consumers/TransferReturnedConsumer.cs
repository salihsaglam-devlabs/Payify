using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.BTrans.Enums;
using LinkPara.SharedModels.BusModels.Commands.Emoney;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class TransferReturnedConsumer : IConsumer<ReturnedWithdrawTransaction>
{
    private readonly ILogger<TransferReturnedConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBus _bus;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IBTransService _bTransService;
    private readonly ILimitService _limitService;
    private readonly IAccountActivityService _accountActivityService;
    private readonly IBulkTransferService _bulkTransferService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;

    public TransferReturnedConsumer(ILogger<TransferReturnedConsumer> logger,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory,
        IBus bus,
        IContextProvider contextProvider,
        IGenericRepository<Account> accountRepository,
        IBTransService bTransService,
        ILimitService limitService,
        IAccountActivityService accountActivityService,
        IBulkTransferService bulkTransferService,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
        _bus = bus;
        _contextProvider = contextProvider;
        _accountRepository = accountRepository;
        _bTransService = bTransService;
        _limitService = limitService;
        _accountActivityService = accountActivityService;
        _bulkTransferService = bulkTransferService;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
    }

    public async Task Consume(ConsumeContext<ReturnedWithdrawTransaction> context)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var request = await dbContext.WithdrawRequest
                        .FirstOrDefaultAsync(q =>
                            q.RecordStatus == RecordStatus.Active &&
                            q.MoneyTransferReferenceId == context.Message.MoneyTransferReferenceId);

        if (request is null)
        {
            _logger.LogError($"WithdrawRequestNotFound! MoneyTransferReferenceId : {context.Message.MoneyTransferReferenceId}");

            return;
        }

        var depositTransactionId = Guid.Empty;

        var strategy = new NoRetryExecutionStrategy(dbContext);

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    request.WithdrawStatus = WithdrawStatus.Failed;
                    request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                    request.UpdateDate = DateTime.Now;

                    dbContext.Update(request);

                    var transaction = await dbContext.Transaction
                        .FirstOrDefaultAsync(s => 
                            s.WithdrawRequestId == request.Id && s.IsReturned == false);

                    var transactionList = new List<Transaction> { transaction };

                    var pricingTransactions = dbContext.Transaction
                        .Where(s => s.RelatedTransactionId == transaction.Id);

                    foreach (var item in pricingTransactions)
                    {
                        transactionList.Add(item);
                    }

                    await SetReturnedFlagAsync(transactionList, dbContext);

                    var depositTransaction = await ReturnToWalletAsync(request, transaction, dbContext);

                    await dbContext.SaveChangesAsync();

                    await transactionScope.CommitAsync();

                    depositTransactionId = depositTransaction.Id;

                    await SendToReturnedTransactionCompletedQueueAsync(
                        new ReturnedTransactionProcessCompleted
                        {
                            IsSucceeded = true,
                            ErrorMessage = string.Empty,
                            MoneyTransferTransactionId = context.Message.MoneyTransferReferenceId
                        });

                    await SendBTransReturnQueueAsync(request, transactionList);

                    await SendBulkTransferQueueAsync(transaction);
                }
                catch (Exception exception)
                {
                    await transactionScope.RollbackAsync();

                    await SendToReturnedTransactionCompletedQueueAsync(
                        new ReturnedTransactionProcessCompleted
                        {
                            IsSucceeded = false,
                            ErrorMessage = exception.Message,
                            MoneyTransferTransactionId = context.Message.MoneyTransferReferenceId
                        });

                    throw;
                }
            });

            await _saveReceiptService.SendReceiptQueueAsync(depositTransactionId);

        }
        catch (Exception exception)
        {
            _logger.LogError($"TransferReturnedConsumer Error : {exception}");
        }
    }

    private Transaction PopulateDepositTransaction(Wallet wallet, Transaction item, Account receiverAccount)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.BankReturn,
            TransactionStatus = Domain.Enums.TransactionStatus.Completed,
            Tag = item.Tag,
            TagTitle = TransactionType.BankReturn.ToString(),
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
            ReturnedTransactionId = item.Id,
            Channel = _contextProvider.CurrentContext?.Channel,
            ReceiverName = receiverAccount != null ? receiverAccount.Name : string.Empty,
        };
    }

    private async Task SendToReturnedTransactionCompletedQueueAsync(ReturnedTransactionProcessCompleted @event)
    {
        try
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.ReturnedTransactionProcessCompleted"));
            await endpoint.Send(@event, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToReturnedTransactionCompletedQueue Error: {exception} - MoneyTransferTransactionId:{@event.MoneyTransferTransactionId}", exception);
        }
    }

    private async Task SendBTransReturnQueueAsync(WithdrawRequest withdrawRequest, List<Transaction> transactions)
    {
        try
        {
            var transaction = transactions.FirstOrDefault(s => !s.Tag.Equals("Bsmv") || !s.Tag.Equals("Pricing"));
            var bsmvTransaction = transactions.FirstOrDefault(s => s.Tag.Equals("Bsmv"));
            var pricingTransaction = transactions.FirstOrDefault(s => s.Tag.Equals("Pricing"));
            var totalPricingAmount = (bsmvTransaction?.Amount ?? 0) + (pricingTransaction?.Amount ?? 0) + (transaction?.Amount ?? 0);

            var receiverAccount = await _accountRepository
                .GetAll()
                .SingleOrDefaultAsync(p => p.Id == transaction.Wallet.AccountId);

            #region MoneyTransferReport
            var receiverBTransIdentity = _bTransService.GetAccountInformation(receiverAccount);
            var moneyTransfer = new MoneyTransferReport
            {
                RecordType = RecordTypeConst.CancelRecord,
                OperationType = OperationType.AccountToAccount,
                TransferType = SharedModels.BusModels.Commands.BTrans.Enums.TransferType.BankToAccount,

                //SenderBlock
                IsSenderCustomer = false,
                IsSenderCorporate = true,
                SenderTaxNumber = withdrawRequest.ReceiverTaxNumber,
                SenderCommercialTitle = withdrawRequest.ReceiverName,
                SenderCityId = 0,
                SenderBankName = withdrawRequest.ReceiverBankName,
                SenderBankCode = withdrawRequest.ReceiverBankCode,
                SenderIbanNumber = withdrawRequest.ReceiverIbanNumber,

                //ReceiverBlock
                IsReceiverCustomer = true,
                IsReceiverCorporate = receiverBTransIdentity.IsCorporate,
                ReceiverPhoneNumber = receiverBTransIdentity.PhoneNumber,
                ReceiverEmail = receiverBTransIdentity.Email,
                ReceiverWalletNumber = withdrawRequest.WalletNumber,
                ReceiverCityId = 0,
                ReceiverTaxNumber = receiverBTransIdentity.TaxNumber,
                ReceiverCommercialTitle = receiverBTransIdentity.CommercialTitle,
                ReceiverFirstName = receiverBTransIdentity.FirstName,
                ReceiverLastName = receiverBTransIdentity.LastName,
                ReceiverIdentityNumber = receiverBTransIdentity.IdentityNumber,

                //TransactionBlock
                RelatedTransactionId = transaction?.Id ?? Guid.Empty,
                TransactionDate = transaction?.TransactionDate ?? DateTime.Now,
                PaymentDate = withdrawRequest.MoneyTransferPaymentDate,
                Amount = transaction?.Amount ?? 0,
                ConvertedAmount = transaction?.Amount ?? 0,
                CurrencyCode = transaction?.CurrencyCode,
                TotalPricingAmount = totalPricingAmount,
                TransferReason = TransferReason.Other,
                IpAddress = _contextProvider.CurrentContext.ClientIpAddress,
                CustomerDescription = transaction?.Description,
            };
            #endregion

            #region ReceiverCustomer
            var receiverCustomerInformation = await _bTransService.GetCustomerInformationAsync(receiverAccount.CustomerId);
            if (receiverCustomerInformation.IsSucceed)
            {
                moneyTransfer.IsReceiverCorporate = receiverCustomerInformation.IsCorporate;
                moneyTransfer.ReceiverPhoneNumber = receiverCustomerInformation.PhoneNumber;
                moneyTransfer.ReceiverEmail = receiverCustomerInformation.Email;
                moneyTransfer.ReceiverNationCountryId = receiverCustomerInformation.NationCountryId;
                moneyTransfer.ReceiverCityId = receiverCustomerInformation.CityId ?? 0;
                moneyTransfer.ReceiverFullAddress = receiverCustomerInformation.FullAddress;
                moneyTransfer.ReceiverDistrict = receiverCustomerInformation.District;
                moneyTransfer.ReceiverPostalCode = receiverCustomerInformation.PostalCode;
                moneyTransfer.ReceiverCity = receiverCustomerInformation.City;
                moneyTransfer.ReceiverTaxNumber = receiverCustomerInformation.TaxNumber;
                moneyTransfer.ReceiverCommercialTitle = receiverCustomerInformation.CommercialTitle;
                moneyTransfer.ReceiverFirstName = receiverCustomerInformation.FirstName;
                moneyTransfer.ReceiverLastName = receiverCustomerInformation.LastName;
                moneyTransfer.ReceiverDocumentType = receiverCustomerInformation.DocumentType;
                moneyTransfer.ReceiverIdentityNumber = receiverCustomerInformation.IdentityNumber;
            }
            #endregion

            await _bTransService.SaveMoneyTransferAsync(moneyTransfer);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to send wallet return with withdrawRequest [{withdrawRequest.Id}] to BTrans reporting tool  Error : {exception}");
        }
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
                Amount = mainTransaction.Amount,
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
            TransactionResult = TransactionResult.Returned
        };
        await _bulkTransferService.SendWithdrawBulkTransferAsync(request);
    }

    private async Task SetReturnedFlagAsync(List<Transaction> transactions, EmoneyDbContext dbContext)
    {
        foreach (Transaction transaction in transactions)
        {
            transaction.IsReturned = true;
        }

        dbContext.UpdateRange(transactions);

        await Task.CompletedTask;
    }
}