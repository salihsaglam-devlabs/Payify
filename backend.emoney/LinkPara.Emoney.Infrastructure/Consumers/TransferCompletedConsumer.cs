using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.Limits;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class TransferCompletedConsumer : IConsumer<TransferCompleted>
{
    private readonly ILogger<TransferCompletedConsumer> _logger;
    private readonly IAccountingService _accountingService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBTransService _bTransService;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pusNotificationSender;
    private readonly ITierLevelService _tierLevelService;
    private readonly IEmailSender _emailSender;
    private readonly ILimitService _limitService;
    private readonly IParameterService _parameterService;
    private readonly IBulkTransferService _bulkTransferService;

    public TransferCompletedConsumer(
        ILogger<TransferCompletedConsumer> logger,
        IAccountingService accountingService,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory,
        IBTransService bTransService,
        IGenericRepository<Account> accountRepository,
        IGenericRepository<Wallet> walletRepository,
        IUserService userService,
        IPushNotificationSender pusNotificationSender,
        ITierLevelService tierLevelService,
        IEmailSender emailSender,
        ILimitService limitService,
        IParameterService parameterService,
        IBulkTransferService bulkTransferService)
    {
        _logger = logger;
        _accountingService = accountingService;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
        _bTransService = bTransService;
        _accountRepository = accountRepository;
        _walletRepository = walletRepository;
        _userService = userService;
        _pusNotificationSender = pusNotificationSender;
        _tierLevelService = tierLevelService;
        _emailSender = emailSender;
        _limitService = limitService;
        _parameterService = parameterService;
        _bulkTransferService = bulkTransferService;
    }

    public async Task Consume(ConsumeContext<TransferCompleted> context)
    {
        Transaction transaction = null;
        
        var request = await GetWithdrawRequestAsync(context.Message.TransactionSourceReferenceId);

        if (request is null)
        {
            return;
        }

        try
        {
            transaction = await GetTransactionAsync(request.InternalTransactionId);

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

            var strategy = new NoRetryExecutionStrategy(dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                var transactions = new List<Transaction> { transaction };

                var wallet = await _walletRepository
                    .GetAll()
                    .SingleOrDefaultAsync(t => t.WalletNumber == request.WalletNumber);
                
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                { 
                        request.WithdrawStatus = WithdrawStatus.Completed;
                        request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                        request.UpdateDate = DateTime.Now;
                        request.MoneyTransferPaymentDate = context.Message.TransferDate;
                        request.MoneyTransferReferenceId = context.Message.MoneyTransferPaymentId;
                        request.MoneyTransferStatus = context.Message.MoneyTransferStatus;

                        dbContext.Update(request);

                        transaction.TransactionStatus = TransactionStatus.Completed;
                        transaction.ExternalTransactionDate = context.Message.TransferDate;
                        transaction.ExternalReferenceId = context.Message.BankReferenceNumber;
                        transaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                        transaction.UpdateDate = DateTime.Now;

                        dbContext.Update(transaction);

                        var pricingTransactions = dbContext.Transaction.Where(s => s.RelatedTransactionId == transaction.Id);
                        foreach (var pricingTransaction in pricingTransactions)
                        {
                            pricingTransaction.TransactionStatus = TransactionStatus.Completed;
                            pricingTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                            pricingTransaction.UpdateDate = DateTime.Now;

                            dbContext.Update(pricingTransaction);
                        }

                        if (pricingTransactions.Any())
                        {
                            transactions.AddRange(pricingTransactions);
                        }

                        var accountTransfer = PopulateAccountTransfer(wallet, request);
                        dbContext.AccountActivity.Add(accountTransfer);
                        
                        await dbContext.SaveChangesAsync();

                        await transactionScope.CommitAsync();
                } 
                catch (Exception)
                {
                    await transactionScope.RollbackAsync();

                    throw;
                }

                var account = _accountRepository.GetAll()
                    .Include(y => y.AccountUsers)
                    .SingleOrDefault(t => t.Id == wallet.AccountId);

                var remainingLimit = await GetAccountRemainingLimit(wallet);

                if (request.IsReceiverIbanOwned)
                {
                    await _tierLevelService.CheckOrUpgradeAccountTierAsync(account, AccountTierValidation.Iban);
                }

                await SendWithdrawBTransQueueAsync(request, transactions, account);

                await SendWithDrawAccountingQueueAsync(request, transactions);

                await SendBulkTransferQueueAsync(transaction);

                _ = Task.Run(() => SendNotificationsAsync(request, wallet, account, remainingLimit));
            });

        }
        catch (Exception exception)
        {
            _logger.LogError($"TransferCompleted Consumer Error {exception}", exception);
        }
    }
    
    private async Task SendNotificationsAsync(WithdrawRequest request, Wallet wallet, Account account, AccountLimitDto remainingLimit)
    {
        try
        {
            var sendPush = await GetNotificationParamAsync("Push");
            var sendMail = await GetNotificationParamAsync("Email");

            if (sendPush || sendMail)
            {
                var withdrawLimit = remainingLimit.Withdraw.MonthlyMaxAmount - remainingLimit.Withdraw.MonthlyUserAmount;
               
                var templateData = new Dictionary<string, string>
                {
                    { "bankName", request.TransactionBankName },
                    { "receiver", request.ReceiverName },
                    { "availableBalance", wallet.AvailableBalance.ToString("N2") },
                    { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                    { "limit", withdrawLimit.ToString("N2") },
                    { "amount", request.Amount.ToString("N2") }
                };

                if (sendPush)
                {
                    await PushSenderNotificationAsync(templateData, account);
                }

                if (sendMail)
                {
                    await SendInformationMailAsync(templateData, account);
                }
            }
            
        }
        catch (Exception exception)
        {
            _logger.LogError("TransferCompleted SendNotificationError for user: {AccountId} {Exception}", account.Id, exception);
        }

    }

    private async Task<AccountLimitDto> GetAccountRemainingLimit(Wallet wallet)
    {
        var remainingLimit = await _limitService.GetAccountLimitsQuery(new GetAccountLimitsQuery
        {
            AccountId = wallet.AccountId,
            CurrencyCode = wallet.CurrencyCode
        });
        return remainingLimit;
    }

    private AccountActivity PopulateAccountTransfer(Wallet senderWallet, WithdrawRequest withdrawRequest)
    {
        return new AccountActivity
        {
            Id = Guid.NewGuid(),
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            LastModifiedBy = _applicationUserService.ApplicationUserId.ToString(),
            RecordStatus = RecordStatus.Active,
            AccountId = senderWallet.AccountId,
            TransferType = PricingCommercialType.Iban.ToString(),
            Sender = $"WA-{senderWallet.WalletNumber}",
            TransactionDirection = TransactionDirection.MoneyOut,
            Receiver = withdrawRequest.ReceiverIbanNumber,
            Amount = withdrawRequest.Amount,
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            OwnAccount = withdrawRequest.IsReceiverIbanOwned
        };
    }

    private async Task PushSenderNotificationAsync(Dictionary<string, string> templateData, Account senderAccount)
    {
        var senderUserList = senderAccount.AccountUsers
            .Select(x =>
            {
                return new NotificationUserInfo
                {
                    UserId = x.UserId,
                    FirstName = x.Firstname,
                    LastName = x.Lastname,
                };
            })
            .ToList();

        var senderUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = senderUserList.Select(x => x.UserId).ToList(),
        });

        var senderNotificationRequest = new SendPushNotification
        {
            TemplateName = "MTWithdrawBankTransfer",
            TemplateParameters = templateData,
            Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = senderUserList
        };

        await _pusNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
    }

    private async Task SendInformationMailAsync(Dictionary<string, string> templateData, Account account)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "MTWithdrawBankTransfer",
            DynamicTemplateData = templateData,
            ToEmail = account.Email
        };

        await _emailSender.SendEmailAsync(mailParams);
    }

    private async Task<WithdrawRequest> GetWithdrawRequestAsync(Guid requestId)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        try
        {
            var request = await dbContext.WithdrawRequest
                    .SingleOrDefaultAsync(s =>
                        s.Id == requestId &&
                        s.RecordStatus == RecordStatus.Active &&
                        s.WithdrawStatus == WithdrawStatus.Delivered);

            if (request is null)
            {
                throw new NotFoundException(nameof(WithdrawRequest), requestId);
            }

            return request;

        }
        catch (Exception exception)
        {
            _logger.LogError($"GetWithdrawRequest Error : {exception}", exception);

            return null;
        }
    }

    private async Task<Transaction> GetTransactionAsync(Guid transactionId)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var transaction = await dbContext.Transaction
            .SingleOrDefaultAsync(s => s.Id == transactionId);

        if (transaction is null)
        {
            throw new NotFoundException(nameof(Transaction));
        }

        if (transaction.TransactionStatus != TransactionStatus.Pending)
        {
            throw new InvalidTransactionStatusException(transaction.Id);
        }

        return transaction;
    }

    private async Task SendWithDrawAccountingQueueAsync(WithdrawRequest request, List<Transaction> transactions)
    {
        var transaction = transactions.FirstOrDefault(s => s.TransactionType == TransactionType.Withdraw);
        var bsmvTransaction = transactions.FirstOrDefault(s => s.TransactionType == TransactionType.Tax);
        var pricingTransaction = transactions.FirstOrDefault(s => s.TransactionType == TransactionType.Commission);

        AccountingPayment payment = new AccountingPayment
        {
            Amount = request.Amount,
            BsmvAmount = bsmvTransaction is not null ? bsmvTransaction.Amount : 0,
            CommissionAmount = pricingTransaction is not null ? pricingTransaction.Amount : 0,
            CurrencyCode = request.CurrencyCode,
            Source = $"WA-{request.WalletNumber}",
            HasCommission = pricingTransaction is not null,
            OperationType = OperationType.Withdraw,
            Destination = "",
            TransactionDate = transaction != null ? transaction.TransactionDate : DateTime.Now,
            BankCode = request.TransactionBankCode,
            UserId = Guid.Parse(request.CreatedBy),
            AccountingCustomerType = AccountingCustomerType.Emoney,
            AccountingTransactionType = AccountingTransactionType.Emoney,
            IbanNumber = request.ReceiverIbanNumber,
            TransactionId = transaction.Id
        };

        await _accountingService.SavePaymentAsync(payment);
    }

    private async Task SendWithdrawBTransQueueAsync(WithdrawRequest withdrawRequest, List<Transaction> transactions, Account senderAccount)
    {
        try
        {
            var transaction = transactions.FirstOrDefault(s => !s.Tag.Equals("Bsmv") || !s.Tag.Equals("Pricing"));
            var bsmvTransaction = transactions.FirstOrDefault(s => s.Tag.Equals("Bsmv"));
            var pricingTransaction = transactions.FirstOrDefault(s => s.Tag.Equals("Pricing"));
            var totalPricingAmount = (bsmvTransaction?.Amount ?? 0) + (pricingTransaction?.Amount ?? 0) + (transaction?.Amount ?? 0);

            #region MoneyTransferReport
            var senderBTransIdentity = _bTransService.GetAccountInformation(senderAccount);
            var moneyTransfer = new MoneyTransferReport
            {
                RecordType = RecordTypeConst.NewRecord,
                OperationType = BTransOperationType.AccountToAccount,
                TransferType = BTransTransferType.AccountToBank,

                //SenderBlock
                IsSenderCustomer = true,
                IsSenderCorporate = senderBTransIdentity.IsCorporate,
                SenderPhoneNumber = senderBTransIdentity.PhoneNumber,
                SenderEmail = senderBTransIdentity.Email,
                SenderWalletNumber = withdrawRequest.WalletNumber,
                SenderCityId = 0,
                SenderTaxNumber = senderBTransIdentity.TaxNumber,
                SenderCommercialTitle = senderBTransIdentity.CommercialTitle,
                SenderFirstName = senderBTransIdentity.FirstName,
                SenderLastName = senderBTransIdentity.LastName,
                SenderIdentityNumber = senderBTransIdentity.IdentityNumber,

                //ReceiverBlock
                IsReceiverCustomer = false,
                IsReceiverCorporate = true,
                ReceiverTaxNumber = withdrawRequest.ReceiverTaxNumber,
                ReceiverCommercialTitle = withdrawRequest.ReceiverName,
                ReceiverCityId = 0,
                ReceiverBankName = withdrawRequest.ReceiverBankName,
                ReceiverBankCode = withdrawRequest.ReceiverBankCode,
                ReceiverIbanNumber = withdrawRequest.ReceiverIbanNumber,

                //TransactionBlock
                RelatedTransactionId = transaction?.Id ?? Guid.Empty,
                TransactionDate = transaction?.TransactionDate ?? DateTime.Now,
                PaymentDate = transaction?.TransactionDate ?? DateTime.Now,
                Amount = transaction?.Amount ?? 0,
                ConvertedAmount = transaction?.Amount ?? 0,
                CurrencyCode = transaction?.CurrencyCode,
                TotalPricingAmount = totalPricingAmount,
                TransferReason = BTransTransferReason.Other,
                IpAddress = string.Empty,
                CustomerDescription = transaction?.Description,
            };
            #endregion

            #region SenderCustomer
            var senderCustomerInformation = await _bTransService.GetCustomerInformationAsync(senderAccount.CustomerId);
            if (senderCustomerInformation.IsSucceed)
            {
                moneyTransfer.IsSenderCorporate = senderCustomerInformation.IsCorporate;
                moneyTransfer.SenderPhoneNumber = senderCustomerInformation.PhoneNumber;
                moneyTransfer.SenderEmail = senderCustomerInformation.Email;
                moneyTransfer.SenderNationCountryId = senderCustomerInformation.NationCountryId;
                moneyTransfer.SenderCityId = senderCustomerInformation.CityId ?? 0;
                moneyTransfer.SenderFullAddress = senderCustomerInformation.FullAddress;
                moneyTransfer.SenderDistrict = senderCustomerInformation.District;
                moneyTransfer.SenderPostalCode = senderCustomerInformation.PostalCode;
                moneyTransfer.SenderCity = senderCustomerInformation.City;
                moneyTransfer.SenderTaxNumber = senderCustomerInformation.TaxNumber;
                moneyTransfer.SenderCommercialTitle = senderCustomerInformation.CommercialTitle;
                moneyTransfer.SenderFirstName = senderCustomerInformation.FirstName;
                moneyTransfer.SenderLastName = senderCustomerInformation.LastName;
                moneyTransfer.SenderDocumentType = senderCustomerInformation.DocumentType;
                moneyTransfer.SenderIdentityNumber = senderCustomerInformation.IdentityNumber;
            }
            #endregion

            await _bTransService.SaveMoneyTransferAsync(moneyTransfer);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to send withdrawRequest [{withdrawRequest.Id}] to BTrans reporting tool  Error : {exception}");
        }
    }

    private async Task<bool> GetNotificationParamAsync(string type)
    {
        var sendNotification = false;

        try
        {
            var param = await _parameterService.GetParameterAsync("NotificationParameters", type);
            sendNotification = bool.Parse(param.ParameterValue);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetNotificationValue Error({type}) : {exception.Message}");
        }

        return sendNotification;
    }

    private async Task SendBulkTransferQueueAsync(Transaction transaction)
    {
        var request = new CheckWithdrawBulkTransferRequest
        {
            TransactionId = transaction.Id,
            TransactionResult = TransactionResult.Success
        };
        await _bulkTransferService.SendWithdrawBulkTransferAsync(request);
    }
}
