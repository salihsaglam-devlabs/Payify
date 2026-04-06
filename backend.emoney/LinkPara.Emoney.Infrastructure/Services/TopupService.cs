using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Transactions;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;
using TransactionType = LinkPara.Emoney.Domain.Enums.TransactionType;

namespace LinkPara.Emoney.Infrastructure.Services;

public class TopupService : ITopupService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TopupService> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly ITierLevelService _tierLevelService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IBTransService _bTransService;
    private readonly IUserService _userService;
    private readonly IEmailSender _emailSender;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IAccountingService _accountingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IParameterService _parameterService;
    private readonly ILimitService _limitService;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IStringLocalizer _notificationLocalizer;
    private const string BatchChannel = "Batch";
    private const string DefaultBankName = "Banka";
    private readonly string _paymentProviderType;
    private readonly IBankAccountService _bankAccountService;
    private readonly string _merchantId;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public TopupService(
        IServiceScopeFactory scopeFactory,
        ILogger<TopupService> logger,
        IVaultClient vaultClient,
        ITierLevelService tierLevelService,
        IApplicationUserService applicationUserService,
        IBTransService bTransService,
        IUserService userService,
        IEmailSender emailSender,
        IPushNotificationSender pushNotificationSender,
        IAccountingService accountingService,
        IAuditLogService auditLogService,
        IParameterService parameterService,
        ILimitService limitService,
        IContextProvider contextProvider,
        IStringLocalizerFactory stringLocalizerFactory,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IBankAccountService bankAccountService,
        IMoneyTransferService moneyTransferService,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService,
        IGenericRepository<Transaction> transactionRepository)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _vaultClient = vaultClient;
        _tierLevelService = tierLevelService;
        _applicationUserService = applicationUserService;
        _bTransService = bTransService;
        _userService = userService;
        _emailSender = emailSender;
        _pushNotificationSender = pushNotificationSender;
        _accountingService = accountingService;
        _auditLogService = auditLogService;
        _parameterService = parameterService;
        _limitService = limitService;
        _paymentProviderType = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
        _contextProvider = contextProvider;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _notificationLocalizer = stringLocalizerFactory.Create("Notifications", "LinkPara.Emoney.API");
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _bankAccountService = bankAccountService;
        _merchantId = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "MerchantId");
        _moneyTransferService = moneyTransferService;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
        _transactionRepository = transactionRepository;
    }

    public async Task<TopupProcessResponse> TopupProcessAsync(TopupProcessCommand topupProcess, Wallet wallet, CardTopupRequest cardTopupRequest, string cardHolderName, decimal amount)
    {
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        await CheckDuplicateTransactionAsync(topupProcess.BaseRequest.CardTopupRequestId);

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var account = await dbContext.Account
            .Include(s => s.AccountUsers)
            .FirstOrDefaultAsync(s => s.Id == wallet.AccountId);

        NullControlHelper.CheckAndThrowIfNull(account, wallet.AccountId, _logger);

        var receiverUser = account.AccountUsers.FirstOrDefault();

        NullControlHelper.CheckAndThrowIfNull(receiverUser, string.Empty, _logger);

        var strategy = dbContext.Database.CreateExecutionStrategy();

        var depositTransaction = new Transaction();
        var accountingTransactions = new List<Transaction>();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var dbWallet = await GetWalletWithLockAsync(dbContext, wallet.Id);

            if (dbWallet is null)
            {
                return;
            }

            depositTransaction = PopulateDepositTransaction(dbWallet, topupProcess, account, amount, cardHolderName);

            dbContext.Add(depositTransaction);
            accountingTransactions.Add(depositTransaction);

            if (cardTopupRequest.CardType == CardType.Credit || cardTopupRequest.CardType == CardType.Unknown)
            {
                dbWallet.CurrentBalanceCredit += amount;
            }
            else
            {
                dbWallet.CurrentBalanceCash += amount;
            }

            dbWallet.LastActivityDate = DateTime.Now;
            dbContext.Update(dbWallet);

            var pricingAmount = Math.Round(cardTopupRequest.Fee + cardTopupRequest.CommissionTotal, 2);

            if (pricingAmount > 0)
            {
                var pricingTransaction = PopulatePricingTransaction(dbWallet, pricingAmount, topupProcess,
                    PaymentMethod.CreditCard, depositTransaction.Id, cardHolderName, account);

                dbContext.Transaction.Add(pricingTransaction);
                accountingTransactions.Add(pricingTransaction);

                Withdraw(dbWallet, pricingAmount, cardTopupRequest.CardType);

                var bsmvTransaction = PopulateBsmvTransaction(dbWallet, cardTopupRequest.BsmvTotal, topupProcess,
                    PaymentMethod.CreditCard, depositTransaction.Id, cardHolderName, account);

                dbContext.Transaction.Add(bsmvTransaction);
                accountingTransactions.Add(bsmvTransaction);

                Withdraw(dbWallet, cardTopupRequest.BsmvTotal, cardTopupRequest.CardType);
            }

            await IncreaseLimitUsageAsync(dbWallet, topupProcess.BaseRequest.Amount, LimitOperationType.Deposit, dbContext);

            await UpdateCardTopupRequest(topupProcess.BaseRequest.CardTopupRequestId, wallet, CardTopupRequestStatus.Completed);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();

            wallet = dbWallet;
        });

        await _saveReceiptService.SendReceiptQueueAsync(depositTransaction.Id);

        await SendDepositAccountingQueueAsync(wallet, accountingTransactions);
        await SendDepositBTransQueueAsync(wallet, account, depositTransaction, cardTopupRequest);

        var details = new Dictionary<string, string>
        {
            { "WalletNumber", wallet.WalletNumber },
            { "TransactionId", depositTransaction.Id.ToString() },
        };

        await SendDepositAuditLogAsync(true, receiverUser.UserId, details);

        var sendPush = await GetNotificationParamAsync("Push");
        var sendEmail = await GetNotificationParamAsync("Email");

        if (sendPush || sendEmail)
        {
            var remainingLimit = await _limitService.GetAccountLimitsQuery(new GetAccountLimitsQuery
            {
                AccountId = wallet.AccountId,
                CurrencyCode = wallet.CurrencyCode
            });

            var depositLimit = remainingLimit.Deposit.MonthlyMaxAmount - remainingLimit.Deposit.MonthlyUserAmount;

            var templateData = new Dictionary<string, string>
            {
                { "cardType", _notificationLocalizer.GetString($"Topup_{cardTopupRequest.CardType}") },
                { "amount", topupProcess.BaseRequest.Amount.ToString("N2") },
                { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
            };

            if (sendPush)
            {
                var user = await _userService.GetUserAsync(receiverUser.UserId);

                _ = Task.Run(() => SendPushNotificationAsync(templateData, user));
            }

            if (sendEmail)
            {
                _ = Task.Run(() => SendInformationMailAsync(templateData, account));
            }
        }

        return new TopupProcessResponse { TransactionId = depositTransaction.Id };
    }

    public async Task TopupReverseAsync(CardTopupRequest request, CardTopupRequestStatus cardTopupRequestStatus, Wallet wallet)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var depositTransaction = new Transaction();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var transaction = await dbContext.Transaction
                .FirstOrDefaultAsync(t => t.CardTopupRequestId == request.Id);

            if (transaction is null)
            {
                throw new NotFoundException(nameof(Transaction), transaction);
            }

            var dbWallet = await GetWalletWithLockAsync(dbContext, transaction.WalletId);

            if (dbWallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            ValidateStatus(dbWallet);

            depositTransaction = await ReturnToWalletAsync(request, dbContext, dbWallet, transaction.Id);

            await UpdateCardTopupRequest(request.Id, wallet, cardTopupRequestStatus);

            await dbContext.SaveChangesAsync();
            transactionScope.Complete();
        });

        await _saveReceiptService.SendReceiptQueueAsync(depositTransaction.Id);
    }

    private async Task UpdateCardTopupRequest(Guid cardTopupRequestId, Wallet wallet,
        CardTopupRequestStatus status)
    {
        if (cardTopupRequestId != Guid.Empty)
        {
            var cardTopupRequest = await _cardTopupRequestRepository.GetByIdAsync(cardTopupRequestId);

            if (cardTopupRequest != null)
            {
                cardTopupRequest.Status = status;
                cardTopupRequest.WalletId = wallet.Id;
                cardTopupRequest.WalletNumber = wallet.WalletNumber;
                cardTopupRequest.Name = wallet.Account.Name;

                await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
            }
        }
    }

    private async Task<Transaction> ReturnToWalletAsync(CardTopupRequest request, EmoneyDbContext dbContext, Wallet wallet, Guid transactionId)
    {
        var transactionList = new List<Transaction>();

        var pricingTransactions = dbContext.Transaction
            .Where(s => s.RelatedTransactionId == transactionId);

        foreach (var item in pricingTransactions)
        {
            transactionList.Add(item);
        }

        var account = await dbContext.Account.FirstOrDefaultAsync(s => s.Id == wallet.AccountId);

        var mainTransaction = WithdrawTransaction(wallet, request, account);
        await dbContext.AddAsync(mainTransaction);

        bool isCredit = request.CardType == CardType.Credit || request.CardType == CardType.Unknown;

        if (isCredit)
        {
            wallet.CurrentBalanceCredit -= mainTransaction.Amount;
        }
        else
        {
            wallet.CurrentBalanceCash -= mainTransaction.Amount;
        }

        foreach (var item in transactionList)
        {
            var newTransaction = PopulateReturnTransaction(wallet, request.Id, account, item, mainTransaction.Id);

            await dbContext.AddAsync(newTransaction);

            if (isCredit)
            {
                wallet.CurrentBalanceCredit += item.Amount;
            }
            else
            {
                wallet.CurrentBalanceCash += item.Amount;
            }
        }

        wallet.LastActivityDate = DateTime.Now;
        wallet.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

        dbContext.Update(wallet);

        return mainTransaction;
    }

    private Transaction PopulatePricingTransaction(Wallet wallet, decimal pricingAmount, TopupProcessCommand topupProcess,
        PaymentMethod paymentMethod, Guid relatedTransactionId, string cardHolderName, Account account = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = TransactionStatus.Completed,
            Tag = account != null
            ? string.Concat(account.Name, " - ", _tagLocalizer.GetString(TransactionType.Commission.ToString()))
            : _tagLocalizer.GetString(TransactionType.Commission.ToString()),
            TagTitle = TransactionType.Commission.ToString(),
            Amount = pricingAmount,
            CurrencyCode = wallet.CurrencyCode,
            WalletId = wallet.Id,
            CreatedBy = topupProcess.BaseRequest.UserId.ToString(),
            CurrentBalance = wallet.AvailableBalance - pricingAmount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = account?.Name,
            SenderName = cardHolderName ?? string.Empty,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulateBsmvTransaction(Wallet wallet, decimal bsmvTotal, TopupProcessCommand topupProcess,
    PaymentMethod paymentMethod, Guid relatedTransactionId, string cardHolderName, Account account = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = TransactionStatus.Completed,
            Tag = account != null
            ? string.Concat(account.Name, " - ", _tagLocalizer.GetString(TransactionType.Tax.ToString()))
            : _tagLocalizer.GetString(TransactionType.Tax.ToString()),
            TagTitle = TransactionType.Tax.ToString(),
            Amount = bsmvTotal,
            CurrencyCode = wallet.CurrencyCode,
            WalletId = wallet.Id,
            CreatedBy = topupProcess.BaseRequest.UserId.ToString(),
            CurrentBalance = wallet.AvailableBalance - bsmvTotal,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = account?.Name,
            SenderName = cardHolderName ?? string.Empty,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private static void ValidateStatus(Wallet wallet)
    {
        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException(wallet.WalletNumber);
        }
    }

    private static void ValidateAccount(Wallet wallet, AccountUser accountUser)
    {
        if (wallet.AccountId != accountUser.AccountId)
        {
            throw new ForbiddenAccessException();
        }
    }

    private async Task SendInformationMailAsync(Dictionary<string, string> templateData, Account account)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "DepositCreditCardTransfer",
            DynamicTemplateData = templateData,
            ToEmail = account.Email
        };

        await _emailSender.SendEmailAsync(mailParams);
    }

    private async Task SendPushNotificationAsync(Dictionary<string, string> templateData, UserDto user)
    {
        var receiverUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = new List<Guid>()
                {
                    user.Id
                },
        });
        var receiverNotificationRequest = new SendPushNotification
        {
            TemplateName = "DepositCreditCardTransfer",
            TemplateParameters = templateData,
            Tokens = receiverUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = new List<NotificationUserInfo>
                        {
                            new NotificationUserInfo
                            {
                                UserId = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                            }
                        }
        };

        await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
    }
    private async Task SendDepositAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "TopupProcess",
                Resource = "Deposit",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
    private async Task SendDepositBTransQueueAsync(Wallet wallet, Account receiverAccount,
                                               Domain.Entities.Transaction transaction,
                                               CardTopupRequest cardTopupRequest)
    {
        try
        {
            #region MoneyTransferReport
            var bTransParameters = await _parameterService.GetParametersAsync("BTransParameters");
            var receiverBTransIdentity = _bTransService.GetAccountInformation(receiverAccount);
            var moneyTransfer = new MoneyTransferReport
            {
                RecordType = RecordTypeConst.NewRecord,
                OperationType = BTransOperationType.AccountToAccount,
                TransferType = BTransTransferType.BankToAccount,

                //SenderBlock
                IsSenderCustomer = false,
                IsSenderCorporate = true,
                SenderTaxNumber = bTransParameters.FirstOrDefault(p => p.ParameterCode == "CorporateTaxNumber")?.ParameterValue,
                SenderCommercialTitle = bTransParameters.FirstOrDefault(p => p.ParameterCode == "CorporateCommercialTitle")?.ParameterValue,
                SenderCityId = 0,
                SenderIbanNumber = string.Empty,
                SenderCreditCard = cardTopupRequest.CardType == CardType.Credit || cardTopupRequest.CardType == CardType.Unknown ? cardTopupRequest.CardNumber : string.Empty,
                SenderDebitCard = cardTopupRequest.CardType == CardType.Debit ? cardTopupRequest.CardNumber : string.Empty,

                ////ReceiverBlock
                IsReceiverCorporate = receiverBTransIdentity.IsCorporate,
                ReceiverPhoneNumber = receiverBTransIdentity.PhoneNumber,
                ReceiverEmail = receiverBTransIdentity.Email,
                ReceiverWalletNumber = wallet.WalletNumber,
                ReceiverCityId = 0,
                ReceiverTaxNumber = receiverBTransIdentity.TaxNumber,
                ReceiverCommercialTitle = receiverBTransIdentity.CommercialTitle,
                ReceiverFirstName = receiverBTransIdentity.FirstName,
                ReceiverLastName = receiverBTransIdentity.LastName,
                ReceiverIdentityNumber = receiverBTransIdentity.IdentityNumber,

                //TransactionBlock
                RelatedTransactionId = transaction.Id,
                TransactionDate = transaction.TransactionDate,
                PaymentDate = transaction.TransactionDate,
                Amount = transaction.Amount,
                ConvertedAmount = transaction.Amount,
                CurrencyCode = transaction.CurrencyCode,
                TotalPricingAmount = transaction.Amount,
                TransferReason = BTransTransferReason.Other,
                CustomerDescription = transaction.Description,
                IpAddress = string.Empty
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
            _logger.LogError(exception, $"Failed to send transaction [{transaction.Id}] to BTrans reporting tool  Error : {exception}");
        }
    }
    private async Task SendDepositAccountingQueueAsync(Wallet wallet, List<Transaction> transactions)
    {
        try
        {
            var bankAccount = await _bankAccountService.GetBankAccountByMerchantId(new GetBankAccountRequest { MerchantId = Guid.Parse(_merchantId) });

            if (bankAccount is null)
            {
                throw new NotFoundException(nameof(MerchantBankAccountDto), _merchantId);
            }

            var checkIbanResponse = await _moneyTransferService.CheckIbanAsync(bankAccount.Iban);

            if (checkIbanResponse is null)
            {
                throw new NotFoundException(nameof(CheckIbanResponse), bankAccount.Iban);
            }
            var transaction = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Deposit);
            var receiverPricing = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Commission);
            var receiverBsmv = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Tax);

            var payment = new AccountingPayment
            {
                Amount = transaction.Amount,
                CurrencyCode = transaction.CurrencyCode,
                Destination = $"WA-{wallet.WalletNumber}",
                Source = "",
                TransactionDate = transaction.TransactionDate,
                BankCode = checkIbanResponse.BankCode,
                OperationType = OperationType.Deposit,
                UserId = Guid.Empty,
                AccountingCustomerType = AccountingCustomerType.Emoney,
                AccountingTransactionType = AccountingTransactionType.CardTopup,
                IbanNumber = string.Empty,
                TransactionId = transaction.Id
            };

            if (receiverPricing is not null && receiverPricing.Amount > 0)
            {
                payment.ReceiverCommissionAmount = receiverPricing.Amount;
                payment.ReceiverBsmvAmount = receiverBsmv.Amount;
                payment.HasCommission = true;
            }

            await _accountingService.SavePaymentAsync(payment);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to send accounting queue [{wallet.WalletNumber}] to accounting Error : {exception}");
        }

    }
    private async Task IncreaseLimitUsageAsync(Wallet wallet, decimal amount, LimitOperationType operationType, EmoneyDbContext dbContext)
    {
        var existingLevel = await dbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == wallet.AccountId
                                       && x.CurrencyCode == wallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(wallet.CurrencyCode, wallet.AccountId,
                _applicationUserService.ApplicationUserId);
            await IncreaseAccountCurrentLevelAsync(wallet, amount, operationType, level);
            dbContext.Add(level);
        }
        else
        {
            await IncreaseAccountCurrentLevelAsync(wallet, amount, operationType, existingLevel);
            dbContext.Update(existingLevel);
        }
    }
    private async Task IncreaseAccountCurrentLevelAsync(Wallet wallet, decimal amount,
        LimitOperationType operationType, AccountCurrentLevel currentLevel)
    {
        await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
        {
            AccountId = wallet.AccountId,
            LimitOperationType = operationType,
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            WalletType = wallet.WalletType
        }, currentLevel);
    }

    private void Withdraw(Wallet wallet, decimal amount, CardType cardType)
    {
        if (cardType == CardType.Credit || cardType == CardType.Unknown)
        {
            if (amount <= wallet.CurrentBalanceCredit)
            {
                wallet.CurrentBalanceCredit -= amount;
            }

        }
        else
        {
            if (amount <= wallet.CurrentBalanceCash)
            {
                wallet.CurrentBalanceCash -= amount;
            }
        }

        wallet.LastActivityDate = DateTime.Now;
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

    private async Task<Wallet> GetWalletWithLockAsync(EmoneyDbContext dbContext, Guid id)
    {
        try
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
        catch (PostgresException exception)
        {
            _logger.LogError("Record is already in progress. It will be retried! Error: {Exception}", exception);

            throw new EntityLockedException();
        }
    }

    private Transaction PopulateReturnTransaction(Wallet wallet, Guid cardTopupRequestId, Account account, Transaction item, Guid mainTransactionId)
    {
        var transaction = new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Return,
            TransactionStatus = TransactionStatus.Completed,
            Tag = string.Empty,
            TagTitle = TransactionType.Return.ToString(),
            Amount = item.Amount,
            CurrencyCode = wallet.CurrencyCode,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            PreBalance = wallet.AvailableBalance,
            CurrentBalance = wallet.AvailableBalance + item.Amount,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.CreditCard,
            RecordStatus = RecordStatus.Active,
            SenderName = account.Name,
            SenderAccountNumber = string.Empty,
            ReceiverName = account.Name,
            Channel = BatchChannel,
            RelatedTransactionId = mainTransactionId
        };
        return transaction;
    }

    private Transaction PopulateDepositTransaction(Wallet wallet, TopupProcessCommand topupProcess, Account account, decimal amount, string cardHolderName)
    {
        var transaction = new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Deposit,
            TransactionStatus = TransactionStatus.Completed,
            Tag = string.Empty,
            TagTitle = TransactionType.Deposit.ToString(),
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            PreBalance = wallet.AvailableBalance,
            CurrentBalance = wallet.AvailableBalance + amount,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.CreditCard,
            RecordStatus = RecordStatus.Active,
            CardTopupRequestId = topupProcess.BaseRequest.CardTopupRequestId,
            SenderName = cardHolderName ?? string.Empty,
            SenderAccountNumber = string.Empty,
            ReceiverName = account.Name,
            Channel = BatchChannel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
        return transaction;
    }

    private Transaction WithdrawTransaction(Wallet wallet, CardTopupRequest request, Account account)
    {
        var amount = Math.Round(request.Amount + request.Fee + request.CommissionTotal + request.BsmvTotal, 2);

        var transaction = new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Withdraw,
            TransactionStatus = TransactionStatus.Completed,
            Tag = string.Empty,
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            PreBalance = wallet.AvailableBalance,
            CurrentBalance = wallet.AvailableBalance - amount,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.CreditCard,
            RecordStatus = RecordStatus.Active,
            CardTopupRequestId = request.Id,
            SenderName = account.Name,
            SenderAccountNumber = string.Empty,
            ReceiverName = account.Name,
            Channel = BatchChannel
        };
        return transaction;
    }

    public async Task CheckDuplicateTransactionAsync(Guid cardTopupRequestId)
    {
        var transactionExists = await _transactionRepository
            .GetAll()
            .AnyAsync(s => s.CardTopupRequestId == cardTopupRequestId && s.TransactionType == TransactionType.Deposit);

        if (transactionExists)
        {
            throw new DuplicateRecordException();
        }
    }
}
