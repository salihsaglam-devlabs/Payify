using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BankingModels.Configurations;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.Emoney;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System.Text.RegularExpressions;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class ProcessIncomingTransactionConsumer : IConsumer<ProcessIncomingTransaction>
{
    private readonly IBus _bus;
    private readonly ILogger<ProcessIncomingTransactionConsumer> _logger;
    private readonly ILimitService _limitService;
    private readonly IAccountingService _accountingService;
    private readonly IAccountIbanService _accountIbanService;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IBankApiService _bankApiService;
    private readonly IFraudTransactionService _transactionService;
    private readonly IParameterService _parameterService;
    private readonly IBTransService _bTransService;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IPricingCommercialService _pricingCommercialService;
    private readonly IVaultClient _vaultClient;
    private readonly ITierLevelService _tierLevelService;
    private readonly IEmailSender _emailSender;
    private readonly IIbanBlacklistService _ibanBlacklistService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;

    private const string BatchChannel = "Batch";
    private const string CommercialDescription = "Ticari Kullanım Ücreti";
    private const string DefaultBankName = "Banka";

    public ProcessIncomingTransactionConsumer(
        IBus bus,
        ILogger<ProcessIncomingTransactionConsumer> logger,
        ILimitService limitService,
        IAccountingService accountingService,
        IAccountIbanService accountIbanService,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory,
        IMoneyTransferService moneyTransferService,
        IBankApiService bankApiService,
        IFraudTransactionService transactionService,
        IParameterService parameterService,
        IBTransService bTransService,
        IUserService userService,
        IPushNotificationSender pushNotificationSender,
        IPricingCommercialService pricingCommercialService,
        IVaultClient vaultClient,
        ITierLevelService tierLevelService,
        IEmailSender emailSender,
        IIbanBlacklistService ibanBlacklistService,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService)
    {
        _bus = bus;
        _logger = logger;
        _limitService = limitService;
        _accountingService = accountingService;
        _accountIbanService = accountIbanService;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
        _moneyTransferService = moneyTransferService;
        _bankApiService = bankApiService;
        _transactionService = transactionService;
        _parameterService = parameterService;
        _bTransService = bTransService;
        _userService = userService;
        _pushNotificationSender = pushNotificationSender;
        _pricingCommercialService = pricingCommercialService;
        _vaultClient = vaultClient;
        _tierLevelService = tierLevelService;

        _emailSender = emailSender;
        _ibanBlacklistService = ibanBlacklistService;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
    }

    public async Task Consume(ConsumeContext<ProcessIncomingTransaction> context)
    {
        var incomingTransaction = context.Message;

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        try
        {
            Wallet wallet = null;

            if (!string.IsNullOrWhiteSpace(context.Message.VirtualIbanNumber))
            {
                var receiverAccount = await dbContext.Account
                    .Include(s => s.Wallets)
                    .FirstOrDefaultAsync(s => s.VirtualIban == context.Message.VirtualIbanNumber);

                if (receiverAccount is null)
                {
                    incomingTransaction.ErrorMessage = "Account not found !";
                }
                else
                {
                    wallet = receiverAccount.Wallets.SingleOrDefault(s => s.IsMainWallet);
                }
            }
            else
            {
                wallet = await ParseWalletNumber(incomingTransaction, dbContext);
            }

            var isValid = Validate(wallet, incomingTransaction);

            var isPermitted = false;

            var validateLimits = false;

            var ownAccount = false;

            if (isValid)
            {
                var isTierExempt = await IsTierExemptFromKkbValidation(wallet.Account.AccountKycLevel);

                if (isTierExempt || !string.IsNullOrWhiteSpace(context.Message.VirtualIbanNumber))
                {
                    incomingTransaction.SenderName = incomingTransaction.SenderName ?? string.Empty;

                    if (wallet.Account != null && (wallet.Account.Name.ToLower() == incomingTransaction.SenderName.ToLower()))
                    {
                        ownAccount = true;
                    }
                }
                else
                {
                    ownAccount = await _accountIbanService.ValidateIbanAsync(wallet.Account.IdentityNumber,
                        incomingTransaction.SenderAccountNumber);
                }

                isPermitted = await ValidatePermissionsAsync(dbContext, wallet, ownAccount, incomingTransaction);
            }

            if (isPermitted)
            {
                validateLimits = await ValidateLimitsAsync(wallet, incomingTransaction);
            }

            if (!isValid || !isPermitted || !validateLimits)
            {
                await SendBackIncomingTransactionAsync(incomingTransaction, wallet, dbContext);
            }
            else
            {
                var ibanBlacklisted = await _ibanBlacklistService.IsBlacklistedAsync(incomingTransaction.SenderAccountNumber);

                if (!ibanBlacklisted)
                {
                    await ProcessIncomingTransactionAsync(dbContext, wallet, incomingTransaction, ownAccount);
                }
                else
                {
                    await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
                    {
                        IncomingTransactionId = incomingTransaction.IncomingTransactionId,
                        Status = IncomingTransactionStatus.ActionRequired,
                        IsSucceeded = false,
                        ErrorMessage = "IBAN blacklisted - Reverse transaction operation not supported"
                    });
                }
            }
        }
        catch (Exception exception)
        {
            if (exception is EntityLockedException)
            {
                throw;
            }

            _logger.LogError($"ProcessIncomingTransactionConsumer Error: {exception}", exception);

            await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
            {
                IncomingTransactionId = incomingTransaction.IncomingTransactionId,
                Status = IncomingTransactionStatus.ActionRequired,
                IsSucceeded = false,
                ErrorMessage = exception.Message
            });

            var details = new Dictionary<string, string>
                    {
                        {"IncomingTransactionId",incomingTransaction.IncomingTransactionId.ToString() },
                        {"IncomingTransactionStatus",IncomingTransactionStatus.ActionRequired.ToString() },
                        {"ExceptionMessage",exception.Message },
                    };

            await SendDepositAuditLogAsync(false, Guid.Empty, details);
        }
    }

    private async Task ProcessIncomingTransactionAsync(EmoneyDbContext dbContext, Wallet wallet,
        ProcessIncomingTransaction incomingTransaction, bool ownAccount)
    {
        var receiverAccount = await dbContext.Account
            .Include(s => s.AccountUsers)
            .FirstOrDefaultAsync(s => s.Id == wallet.AccountId);

        NullControlHelper.CheckAndThrowIfNull(receiverAccount, wallet.AccountId, _logger);

        var receiverUser = receiverAccount.AccountUsers.FirstOrDefault();

        NullControlHelper.CheckAndThrowIfNull(receiverUser, string.Empty, _logger);

        var currencyCode = await GetCurrencyCode(incomingTransaction.CurrencyCode);

        var IsTransactionCheckEnabled =
           _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "TransactionEnabled");

        var checkFraud = false;

        if (IsTransactionCheckEnabled)
        {
            var requestModel = new FraudTransactionDetail
            {
                Amount = incomingTransaction.Amount,
                BeneficiaryNumber = wallet.WalletNumber,
                Beneficiary = receiverAccount.Name,
                OriginatorNumber = incomingTransaction.SenderAccountNumber,
                Originator = incomingTransaction.SenderName,
                FraudSource = FraudSource.Wallet,
                Direction = Direction.Inbound,
                AmountCurrencyCode = Convert.ToInt32(currencyCode),
                BeneficiaryAccountCurrencyCode = Convert.ToInt32(currencyCode),
                OriginatorAccountCurrencyCode = Convert.ToInt32(currencyCode),
                Channel = BatchChannel
            };

            checkFraud = await CheckFraudAsync(new FraudCheckRequest
            {
                CommandName = "Deposit",
                ExecuteTransaction = requestModel,
                UserId = receiverUser.UserId.ToString(),
                Module = "Emoney",
                AccountKycLevel = receiverAccount.AccountKycLevel,
                CommandJson = JsonConvert.SerializeObject(requestModel)
            }, incomingTransaction.IncomingTransactionId);
        }

        if (!checkFraud)
        {
            var depositTransaction = new Transaction();

            var strategy = new NoRetryExecutionStrategy(dbContext);

            var commercialPricingDefinition =
                await _pricingCommercialService.GetPricingCommercialRateAsync(wallet.CurrencyCode, PricingCommercialType.Iban);

            var isGreaterThanMinAmountLimit =
                await _pricingCommercialService.IsGreaterThanMinAmountLimit(incomingTransaction.Amount);

            CalculatePricingResponse receiverPricing = null;

            if (commercialPricingDefinition is not null && receiverAccount.IsCommercial && isGreaterThanMinAmountLimit)
            {
                receiverPricing =
                    await _pricingCommercialService.CalculateCustomPricingAsync(incomingTransaction.Amount, 0,
                        commercialPricingDefinition.CommissionRate);
            }

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                if (incomingTransaction.IncomingTransactionId != Guid.Empty)
                {
                    var duplicateTransaction = await dbContext.Transaction
                    .AnyAsync(s => s.IncomingTransactionId == incomingTransaction.IncomingTransactionId);

                    if (duplicateTransaction)
                    {
                        _logger.LogError($"ProcessIncomingTransactionConsumer Duplicate Transaction Error : IncomingTransactionId - {incomingTransaction.IncomingTransactionId}");

                        return;
                    }
                }

                try
                {
                    var dbWallet = await GetWalletWithLockAsync(dbContext, wallet.Id);

                    if (dbWallet is null)
                    {
                        return;
                    }

                    depositTransaction = PopulateDepositTransaction(dbWallet, incomingTransaction, receiverAccount);
                    dbContext.Add(depositTransaction);

                    dbWallet.CurrentBalanceCash += incomingTransaction.Amount;
                    dbWallet.LastActivityDate = DateTime.Now;
                    dbContext.Update(dbWallet);

                    var accountActivity =
                        PopulateAccountActivity(dbWallet, incomingTransaction, receiverAccount, ownAccount);
                    dbContext.AccountActivity.Add(accountActivity);

                    if (!ownAccount)
                    {
                        if (receiverAccount.IsCommercial && isGreaterThanMinAmountLimit)
                        {
                            await ApplyCommercialPricing(incomingTransaction, depositTransaction, dbWallet, commercialPricingDefinition, dbContext);
                        }
                    }
                    else
                    {
                        await _tierLevelService.CheckOrUpgradeAccountTierAsync(receiverAccount, AccountTierValidation.Iban);
                    }

                    await IncreaseLimitUsageAsync(dbWallet, incomingTransaction.Amount, LimitOperationType.Deposit, dbContext);

                    await dbContext.SaveChangesAsync();

                    await transactionScope.CommitAsync();

                    wallet = dbWallet;
                }
                catch (Exception exception)
                {
                    _logger.LogError($"ProcessIncomingTransactionConsumer Transaction Error: {exception}", exception);

                    await transactionScope.RollbackAsync();

                    throw;
                }
            });

            await CheckAccountCurrentCommercialStatus(dbContext, wallet, receiverAccount, ownAccount);

            await SendDepositAccountingQueueAsync(wallet, incomingTransaction, receiverPricing, depositTransaction, ownAccount);

            await SendDepositBTransQueueAsync(wallet, receiverAccount, incomingTransaction, depositTransaction);

            await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
            {
                IncomingTransactionId = incomingTransaction.IncomingTransactionId,
                Status = IncomingTransactionStatus.Completed,
                IsSucceeded = true,
                ErrorMessage = "",
                ReceiverName = receiverAccount.Name,
                ReceiverWalletNumber = wallet.WalletNumber
            });

            await _saveReceiptService.SendReceiptQueueAsync(depositTransaction.Id);

            var details = new Dictionary<string, string>
                {
                { "IncomingWalletNumber", wallet.WalletNumber },
                { "IncomingTransactionId", incomingTransaction.IncomingTransactionId.ToString() },
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

                var bankName = DefaultBankName;

                if (!string.IsNullOrEmpty(incomingTransaction.SenderBankName))
                {
                    bankName = incomingTransaction.SenderBankName;
                }

                var templateData = new Dictionary<string, string>
                {
                    { "bankName", bankName },
                    { "sender", incomingTransaction.SenderName },
                    { "amount", incomingTransaction.Amount.ToString("N2") },
                    { "limit", depositLimit.ToString("N2") },
                    { "availableBalance", wallet.AvailableBalance.ToString("N2") },
                    { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                };

                if (sendPush)
                {
                    var user = await _userService.GetUserAsync(receiverUser.UserId);

                    _ = Task.Run(() => SendPushNotificationAsync(templateData, user));
                }

                if (sendEmail)
                {
                    _ = Task.Run(() => SendInformationMailAsync(templateData, receiverAccount));
                }
            }
        }
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

    private async Task CheckAccountCurrentCommercialStatus(EmoneyDbContext dbContext, Wallet wallet, Account receiverAccount, bool ownAccount)
    {
        var isCommercialNow =
            await _pricingCommercialService.CheckIfAccountIsCommercialNowAsync(wallet.CurrencyCode, PricingCommercialType.Iban, receiverAccount.Id, ownAccount);

        if (!receiverAccount.IsCommercial && isCommercialNow)
        {
            receiverAccount.IsCommercial = true;
            dbContext.Account.Update(receiverAccount);
            await dbContext.SaveChangesAsync();

            var pricingRate = await _pricingCommercialService.GetPricingCommercialRateAsync(wallet.CurrencyCode, PricingCommercialType.Iban);

            await _pricingCommercialService.SendCommercialInfoPushNotificationAsync(receiverAccount, pricingRate);
        }
    }

    private async Task SendBackIncomingTransactionAsync(ProcessIncomingTransaction incomingTransaction, Wallet wallet, EmoneyDbContext dbContext)
    {
        var bankApi = await _bankApiService.GetByBankCodeAsync(incomingTransaction.ReceiverBankCode);

        if (incomingTransaction.ErrorMessage == ErrorMessages.WalletIsNotActive ||
            bankApi is null ||
            !bankApi.ProcessReverseTransfer)
        {
            await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
            {
                IncomingTransactionId = incomingTransaction.IncomingTransactionId,
                Status = IncomingTransactionStatus.ActionRequired,
                IsSucceeded = false,
                ErrorMessage = $"{incomingTransaction.ErrorMessage} - Reverse transaction operation not supported"
            });

            var details = new Dictionary<string, string>
            {
                { "IncomingWalletNumber", wallet is null ? "NotFound" : wallet.WalletNumber },
                { "IncomingTransactionId", incomingTransaction.IncomingTransactionId.ToString() },
                { "IncomingTransactionStatus", IncomingTransactionStatus.ActionRequired.ToString() },
            };

            await SendDepositAuditLogAsync(false, _applicationUserService.ApplicationUserId, details);
        }
        else
        {
            var transferBank = await _moneyTransferService.GetTransferBankAsync(new GetTransferBankRequest
            {
                Amount = incomingTransaction.Amount,
                ReceiverIBAN = incomingTransaction.SenderAccountNumber,
                CurrencyCode = incomingTransaction.CurrencyCode,
            });

            var returnTransactionRequest = PopulateReturnTransactionRequest(incomingTransaction, transferBank);

            dbContext.Add(returnTransactionRequest);

            await dbContext.SaveChangesAsync();

            await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
            {
                IncomingTransactionId = incomingTransaction.IncomingTransactionId,
                Status = IncomingTransactionStatus.WaitingReturnToBank,
                IsSucceeded = false,
                ErrorMessage = incomingTransaction.ErrorMessage
            });

            var details = new Dictionary<string, string>
            {
                { "IncomingWalletNumber", wallet is null ? "NotFound" : wallet.WalletNumber },
                { "IncomingTransactionId", incomingTransaction.IncomingTransactionId.ToString() },
                { "IncomingTransactionStatus", IncomingTransactionStatus.WaitingReturnToBank.ToString() },
            };

            await SendDepositAuditLogAsync(false, _applicationUserService.ApplicationUserId, details);
        }
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
            TemplateName = "MTDepositBankTransfer",
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

    private async Task SendInformationMailAsync(Dictionary<string, string> templateData, Account account)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "MTDepositBankTransfer",
            DynamicTemplateData = templateData,
            ToEmail = account.Email
        };

        await _emailSender.SendEmailAsync(mailParams);
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

    private async Task<bool> CheckFraudAsync(FraudCheckRequest requestFraud, Guid IncomingTransactionId)
    {
        try
        {
            var transaction = await _transactionService.ExecuteTransaction(requestFraud);
            if (transaction.IsSuccess)
            {
                int riskLevel;
                try
                {
                    var parameter = await _parameterService.GetParameterAsync("FraudParameters", "RiskLevel");
                    riskLevel = Convert.ToInt32(parameter.ParameterValue);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"GetParameterAsync Error : {exception} ");
                    riskLevel = (int)RiskLevel.Critical;
                }

                if ((int)transaction.RiskLevel >= riskLevel)
                {
                    _logger.LogError($"ProcessIncomingTransactionConsumer Error: Fraud Check Error");

                    await SendToTransactionCompletedQueueAsync(new IncomingTransactionProcessCompleted
                    {
                        IncomingTransactionId = IncomingTransactionId,
                        Status = IncomingTransactionStatus.ActionRequired,
                        IsSucceeded = false,
                        ErrorMessage = $"Fraud Check Risk Level: {transaction.RiskLevel}"
                    });
                    return true;
                }
            }
            else
            {
                _logger.LogError($"Fraud Transfer Error : {transaction.ErrorMessage}");
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Fraud Transfer Error : {exception}");
        }
        return false;
    }

    private async Task<string> GetCurrencyCode(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
        .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);

        return parameterTemplateValue?.FirstOrDefault(b => b.TemplateCode == "Number")?.TemplateValue;
    }

    private async Task SendToTransactionCompletedQueueAsync(IncomingTransactionProcessCompleted @event)
    {
        try
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.IncomingTransactionProcessCompleted"));
            await endpoint.Send(@event, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToTransactionCompletedQueue Error: {exception} - IncomingTransactionId:{@event.IncomingTransactionId}", exception);
        }
    }

    private async Task SendDepositAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "ProcessIncomingTransaction",
                Resource = "Deposit",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }

    private async Task SendDepositAccountingQueueAsync(Wallet wallet, ProcessIncomingTransaction transaction, CalculatePricingResponse receiverPricing, Transaction depositTransaction, bool ownAccount)
    {

        var payment = new AccountingPayment
        {
            Amount = depositTransaction.Amount,
            CurrencyCode = depositTransaction.CurrencyCode,
            Destination = $"WA-{wallet.WalletNumber}",
            Source = "",
            TransactionDate = depositTransaction.TransactionDate,
            BankCode = transaction.ReceiverBankCode,
            OperationType = OperationType.Deposit,
            UserId = Guid.Empty,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            AccountingTransactionType = string.IsNullOrWhiteSpace(transaction.VirtualIbanNumber)
                                            ? AccountingTransactionType.Emoney
                                            : AccountingTransactionType.VirtualIban,
            IbanNumber = transaction.SenderAccountNumber,
            TransactionId = depositTransaction.Id
        };

        if (receiverPricing is not null && receiverPricing.CommissionAmount > 0 && !ownAccount)
        {
            payment.ReceiverCommissionAmount = receiverPricing.CommissionAmount;
            payment.ReceiverBsmvAmount = receiverPricing.BsmvTotal;
            payment.HasCommission = true;
        }

        await _accountingService.SavePaymentAsync(payment);
    }

    private async Task SendDepositBTransQueueAsync(Wallet wallet, Account receiverAccount,
                                                   ProcessIncomingTransaction incomingTransaction,
                                                   Transaction transaction)
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
                SenderBankName = incomingTransaction.SenderBankName,
                SenderBankCode = incomingTransaction.SenderBankCode,
                SenderIbanNumber = incomingTransaction.SenderAccountNumber,

                //ReceiverBlock
                IsReceiverCustomer = true,
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
            _logger.LogError(exception, $"Failed to send incoming transaction [{transaction.Id}] to BTrans reporting tool  Error : {exception}");
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

    private async Task<Wallet> ParseWalletNumber(ProcessIncomingTransaction transaction, EmoneyDbContext context)
    {
        var walletNumbers = Regex.Matches(transaction.Description.Trim(), @"\b[0-9]{10}\b");

        if (!walletNumbers.Any())
        {
            transaction.ErrorMessage = "WalletInfo does not exists in comment";
            return null;
        }

        var potentialWalletNumbers = walletNumbers.Select(s => s.Value);

        var walletsInDatabase = await context.Wallet
            .Where(s =>
                potentialWalletNumbers.Contains(s.WalletNumber) &&
                s.RecordStatus == RecordStatus.Active)
            .Include(s => s.Account)
            .ToListAsync();

        if (walletsInDatabase.Count > 1)
        {
            transaction.ErrorMessage = "Potential multiple wallet information in comment. !";
            return null;
        }

        if (walletsInDatabase.Count == 0)
        {
            transaction.ErrorMessage = "Wallet not found !";
            return null;
        }

        return walletsInDatabase.FirstOrDefault();
    }

    private async Task ApplyCommercialPricing(
        ProcessIncomingTransaction incomingTransaction,
        Transaction transaction,
        Wallet receiverWallet,
        PricingCommercial pricingCommercial,
        EmoneyDbContext dbContext)
    {
        if (pricingCommercial is not null)
        {
            var customPricing = await _pricingCommercialService.CalculateCustomPricingAsync(incomingTransaction.Amount,
                decimal.Zero, pricingCommercial.CommissionRate);

            var pricingTransaction = PopulateCommercialUsageTransaction(receiverWallet, customPricing, transaction.Id);
            dbContext.Transaction.Add(pricingTransaction);
            Withdraw(receiverWallet, customPricing.CommissionAmount);

            var bsmvTransaction = PopulateCommercialUsageBsmvTransaction(receiverWallet, customPricing, PaymentMethod.BankTransfer, transaction.Id);
            dbContext.Transaction.Add(bsmvTransaction);
            Withdraw(receiverWallet, customPricing.BsmvTotal);
        }
    }

    private void Withdraw(Wallet wallet, decimal amount)
    {
        if (amount <= wallet.CurrentBalanceCredit)
        {
            wallet.CurrentBalanceCredit -= amount;
        }
        else
        {
            var difference = amount - wallet.CurrentBalanceCredit;
            wallet.CurrentBalanceCredit = 0;
            wallet.CurrentBalanceCash -= difference;
        }

        wallet.LastActivityDate = DateTime.Now;
    }

    private static bool Validate(Wallet wallet, ProcessIncomingTransaction transaction)
    {
        if (!string.IsNullOrWhiteSpace(transaction.ErrorMessage))
        {
            return false;
        }

        if (wallet is null)
        {
            transaction.ErrorMessage = ErrorMessages.WalletNotFound;
        }

        else if (wallet.RecordStatus == RecordStatus.Passive || wallet.IsBlocked)
        {
            transaction.ErrorMessage = ErrorMessages.WalletIsNotActive;
        }

        else if (wallet.CurrencyCode != transaction.CurrencyCode)
        {
            transaction.ErrorMessage = ErrorMessages.CurrencyCodeError;
        }

        return string.IsNullOrWhiteSpace(transaction.ErrorMessage);
    }

    private async Task<bool> ValidateLimitsAsync(Wallet wallet, ProcessIncomingTransaction transaction)
    {
        var response = await _limitService.IsLimitExceededAsync(new LimitControlRequest
        {
            Amount = transaction.Amount,
            CurrencyCode = transaction.CurrencyCode,
            LimitOperationType = LimitOperationType.Deposit,
            AccountId = wallet.AccountId,
            WalletNumber = wallet.WalletNumber
        });

        if (response.IsLimitExceeded)
        {
            transaction.ErrorMessage = "Limit exceeded.";
            return false;
        }
        return true;
    }

    private Transaction PopulateDepositTransaction(Wallet wallet, ProcessIncomingTransaction item, Account receiverAccount)
    {
        var tran = new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Deposit,
            TransactionStatus = TransactionStatus.Completed,
            Tag = item.SenderName,
            TagTitle = TransactionType.Deposit.ToString(),
            Amount = item.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = item.Description,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            PreBalance = wallet.AvailableBalance,
            CurrentBalance = wallet.AvailableBalance + item.Amount,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.BankTransfer,
            RecordStatus = RecordStatus.Active,
            SenderBankCode = item.SenderBankCode,
            SenderName = item.SenderName,
            SenderAccountNumber = item.SenderAccountNumber,
            ReceiverName = receiverAccount.Name,
            IncomingTransactionId = item.IncomingTransactionId,
            Channel = BatchChannel,
            ExternalReferenceId = item.BankReferenceNumber,
            ExternalTransactionDate = item.TransactionDate
        };
        return tran;
    }

    private ReturnTransactionRequest PopulateReturnTransactionRequest(ProcessIncomingTransaction item, GetTransferBankResponse transferInfo)
    {
        var tenant = Environment.GetEnvironmentVariable("Tenant");

        return new ReturnTransactionRequest
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = item.Amount,
            CurrencyCode = item.CurrencyCode,
            ReceiverBankCode = item.SenderBankCode,
            ReceiverIbanNumber = item.SenderAccountNumber,
            ReceiverName = item.SenderName,
            ReceiverTaxNumber = item.SenderTaxNumber,
            Description = $"{tenant} - Iade",
            Status = ReturnTransactionStatus.Pending,
            TransactionBankCode = transferInfo.TransferBankCode,
            IncomingTransactionId = item.IncomingTransactionId,
            RecordStatus = RecordStatus.Active,
            TransferType = transferInfo.TransferType
        };
    }

    private Transaction PopulateCommercialUsageTransaction(Wallet wallet, CalculatePricingResponse customPricing,
                                                            Guid relatedTransactionId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = TransactionStatus.Completed,
            Tag = CommercialDescription,
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = customPricing.CommissionAmount,
            CurrencyCode = wallet.CurrencyCode,
            Description = CommercialDescription,
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            CurrentBalance = wallet.AvailableBalance - customPricing.CommissionAmount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.BankTransfer,
            RecordStatus = RecordStatus.Active,
            ReceiverBankCode = null,
            ReceiverName = string.Empty,
            Channel = BatchChannel,
            SenderName = string.Empty,
            RelatedTransactionId = relatedTransactionId
        };
    }

    private Transaction PopulateCommercialUsageBsmvTransaction
    (
        Wallet wallet,
        CalculatePricingResponse pricing,
        PaymentMethod paymentMethod,
        Guid relatedTransactionId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = TransactionStatus.Completed,
            Tag = CommercialDescription,
            TagTitle = TransactionType.Tax.ToString(),
            Amount = pricing.BsmvTotal,
            CurrencyCode = wallet.CurrencyCode,
            Description = $"{CommercialDescription} Bsmv",
            WalletId = wallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            CurrentBalance = wallet.AvailableBalance - pricing.BsmvTotal,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = string.Empty,
            SenderName = string.Empty,
            Channel = BatchChannel
        };
    }

    private AccountActivity PopulateAccountActivity(Wallet receiverWallet, ProcessIncomingTransaction incomingTransaction, Account receiverAccount, bool ownAccount)
    {
        return new AccountActivity
        {
            Id = Guid.NewGuid(),
            AccountId = receiverAccount.Id,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            LastModifiedBy = _applicationUserService.ApplicationUserId.ToString(),
            RecordStatus = RecordStatus.Active,
            TransferType = PricingCommercialType.Iban.ToString(),
            Sender = incomingTransaction.SenderAccountNumber ?? string.Empty,
            TransactionDirection = TransactionDirection.MoneyIn,
            Receiver = $"WA-{receiverWallet.WalletNumber}",
            Amount = incomingTransaction.Amount,
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            OwnAccount = ownAccount
        };
    }

    private async Task<bool> ValidatePermissionsAsync(EmoneyDbContext dbContext, Wallet wallet, bool ownAccount, ProcessIncomingTransaction transaction)
    {
        var permission = await
            dbContext.TierPermission
                .Where(tp =>
                    tp.TierLevel == GetTierLevelType(wallet.Account.AccountKycLevel) &&
                    tp.PermissionType == (ownAccount ? TierPermissionType.DepositFromOwnIban : TierPermissionType.DepositFromOtherIban))
                .SingleOrDefaultAsync();
        var isPermitted = permission is null || permission.IsEnabled;
        if (!isPermitted)
        {
            transaction.ErrorMessage = "Tier level permission is not valid";
        }
        return isPermitted;
    }

    private static TierLevelType GetTierLevelType(AccountKycLevel accountKycLevel)
    {
        return accountKycLevel switch
        {
            AccountKycLevel.NoneKyc => TierLevelType.Tier0,
            AccountKycLevel.PreKyc => TierLevelType.Tier1,
            AccountKycLevel.Kyc => TierLevelType.Tier2,
            AccountKycLevel.Premium => TierLevelType.Tier3,
            AccountKycLevel.PremiumPlus => TierLevelType.Tier4,
            AccountKycLevel.ChildKyc => TierLevelType.Tier5,
            AccountKycLevel.CorporateKyc => TierLevelType.Corporate,
            _ => TierLevelType.Custom
        };
    }

    private static class ErrorMessages
    {
        public const string WalletIsNotActive = "Wallet is not active.";
        public const string WalletNotFound = "Wallet not found.";
        public const string CurrencyCodeError = "Currency codes are different.";
    }

    private async Task<bool> IsTierExemptFromKkbValidation(AccountKycLevel accountKycLevel)
    {
        try
        {
            var incomingSettings = await _vaultClient.GetSecretValueAsync<IncomingTransactionSettings>("EmoneySecrets", "IncomingTransactionSettings");

            if (incomingSettings == null)
            {
                return false;
            }

            var exemptTiersForKkbValidation = incomingSettings.ExemptTiersForKkbValidation ?? new List<string>();

            var accountTier = GetTierLevelType(accountKycLevel);

            return exemptTiersForKkbValidation.Contains(accountTier.ToString());
        }
        catch
        {
            return false;
        }
    }
}