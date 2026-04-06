using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.BankingModels;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Application.Features.Wallets.Queries;
using LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;
using LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.BusinessParameter.Models;
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
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;

namespace LinkPara.Emoney.Infrastructure.Services;

public class TransferService : ITransferService
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IPricingCommercialService _pricingCommercialService;
    private readonly ILogger<TransferService> _logger;
    private readonly IPricingProfileService _pricingProfileService;
    private readonly ILimitService _limitService;
    private readonly IUserService _userService;
    private readonly IAccountingService _accountingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IFraudTransactionService _transactionService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IBTransService _bTransService;
    private readonly IPushNotificationSender _pusNotificationSender;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IStringLocalizer _exceptionLocalizer;
    private readonly IVaultClient _vaultClient;
    private readonly ITierLevelService _tierLevelService;
    private readonly IAccountIbanService _accountIbanService;
    private readonly ITierPermissionService _permissionService;
    private readonly IAccountActivityService _accountActivityService;
    private readonly IEmailSender _emailSender;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;
    private readonly ICashbackService _cashbackService;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private const string NoneKYCWithdrawalLimit = "NoneKYCWithdrawalLimit";
    private const string MoneyTransferParameterGroup = "MoneyTransferPaymentType";

    public TransferService(IGenericRepository<Wallet> walletRepository,
        IPricingCommercialService pricingCommercialService,
        ILogger<TransferService> logger,
        IPricingProfileService pricingProfileService,
        ILimitService limitService,
        IUserService userService,
        IAccountingService accountingService,
        IAuditLogService auditLogService,
        IMoneyTransferService moneyTransferService,
        IFraudTransactionService transactionService,
        IContextProvider contextProvider,
        IGenericRepository<Account> accountRepository,
        IGenericRepository<AccountUser> accountUserRepository,
        IParameterService parameterService,
        IBTransService bTransService,
        IPushNotificationSender pusNotificationSender,
        IServiceScopeFactory scopeFactory,
        IStringLocalizerFactory stringLocalizerFactory,
        IVaultClient vaultClient,
        ITierLevelService tierLevelService,
        IAccountIbanService accountIbanService,
        ITierPermissionService permissionService,
        IAccountActivityService accountActivityService,
        IEmailSender emailSender,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService,
        ICashbackService cashbackService,
        IGenericRepository<Transaction> transactionRepository)
    {
        _walletRepository = walletRepository;
        _pricingCommercialService = pricingCommercialService;
        _logger = logger;
        _pricingProfileService = pricingProfileService;
        _limitService = limitService;
        _userService = userService;
        _accountingService = accountingService;
        _auditLogService = auditLogService;
        _moneyTransferService = moneyTransferService;
        _transactionService = transactionService;
        _contextProvider = contextProvider;
        _accountRepository = accountRepository;
        _accountUserRepository = accountUserRepository;
        _parameterService = parameterService;
        _bTransService = bTransService;
        _pusNotificationSender = pusNotificationSender;
        _scopeFactory = scopeFactory;
        _tierLevelService = tierLevelService;
        _accountIbanService = accountIbanService;
        _permissionService = permissionService;
        _accountActivityService = accountActivityService;

        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _exceptionLocalizer = stringLocalizerFactory.Create("Exceptions", "LinkPara.Emoney.API");
        _vaultClient = vaultClient;
        _emailSender = emailSender;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
        _cashbackService = cashbackService;
        _transactionRepository = transactionRepository;
    }

    public async Task<MoneyTransferResponse> TransferAsync(TransferCommand request,
        CancellationToken cancellationToken)
    {
        request.Amount = request.Amount.ToDecimal2();

        var senderWallet = await _walletRepository.GetAll()
            .Include(t => t.Account)
            .SingleOrDefaultAsync(t => t.WalletNumber == request.SenderWalletNumber,
                cancellationToken: cancellationToken);

        if (senderWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.SenderWalletNumber);
        }

        var receiverWallet = await _walletRepository.GetAll()
            .Include(t => t.Account)
            .SingleOrDefaultAsync(t => t.WalletNumber == request.ReceiverWalletNumber,
                cancellationToken: cancellationToken);

        if (receiverWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.ReceiverWalletNumber);
        }

        ValidateWallets(senderWallet, receiverWallet);

        var isSameAccount = senderWallet.AccountId == receiverWallet.AccountId;

        if (!isSameAccount)
        {
            await ValidateDescriptionLengthAsync(request.PaymentType, request.Description);
            await _permissionService.ValidatePermissionAsync(senderWallet.Account.AccountKycLevel, TierPermissionType.P2PTransfer);
        }

        ValidateBalance(senderWallet, request.Amount, isSameAccount: isSameAccount);

        var pricing = await CalculatePricingAsync(senderWallet, receiverWallet, request.Amount);

        var senderAccount = await _accountRepository
            .GetAll()
            .Include(p => p.AccountUsers)
            .SingleOrDefaultAsync(p => p.Id == senderWallet.AccountId, cancellationToken: cancellationToken);

        var receiverAccount = await _accountRepository
            .GetAll()
            .Include(p => p.AccountUsers)
            .SingleOrDefaultAsync(p => p.Id == receiverWallet.AccountId, cancellationToken: cancellationToken);

        var currencyCode = await GetCurrencyCodeAsync(senderWallet.CurrencyCode);

        await CheckDuplicateTransactionAsync(request.IdempotentKey);

        var IsTransactionCheckEnabled = _vaultClient
            .GetSecretValue<bool>("/SharedSecrets", "ServiceState", "TransactionEnabled");

        if (IsTransactionCheckEnabled)
        {
            var requestModel = new FraudTransactionDetail
            {
                Amount = request.Amount,
                BeneficiaryNumber = request.ReceiverWalletNumber,
                Beneficiary = receiverAccount.Name,
                OriginatorNumber = request.SenderWalletNumber,
                Originator = senderAccount.Name,
                FraudSource = FraudSource.Wallet,
                Direction = Direction.Outbound,
                AmountCurrencyCode = Convert.ToInt32(currencyCode),
                BeneficiaryAccountCurrencyCode = Convert.ToInt32(currencyCode),
                OriginatorAccountCurrencyCode = Convert.ToInt32(currencyCode),
                Channel = _contextProvider.CurrentContext.Channel
            };

            await CheckFraudAsync(new FraudCheckRequest
            {
                CommandName = "P2P",
                ExecuteTransaction = requestModel,
                UserId = request.UserId,
                Module = "Emoney",
                AccountKycLevel = senderAccount.AccountKycLevel,
                CommandJson = JsonConvert.SerializeObject(requestModel)
            });
        }

        return await ExecuteTransferAsync(senderWallet, receiverWallet, request,
            senderAccount, receiverAccount, pricing, isSameAccount);
    }

    private async Task<string> GetCurrencyCodeAsync(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
         .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);

        return parameterTemplateValue?.FirstOrDefault(b => b.TemplateCode == "Number")?.TemplateValue;
    }

    private async Task CheckFraudAsync(FraudCheckRequest requestFraud)
    {
        var transaction = new TransactionResponse();
        try
        {
            transaction = await _transactionService.ExecuteTransaction(requestFraud);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Fraud Transfer Error : {exception}");
        }

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
                var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

                var exceptionMessage = _exceptionLocalizer.GetString("PotentialFraudException");

                throw new PotentialFraudException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
            }
        }
        else
        {
            _logger.LogError($"Fraud Transfer Error : {transaction.ErrorMessage}");
        }
    }

    public async Task<TransferPreviewResponse> TransferPreviewAsync(TransferPreviewQuery request)
    {
        request.Amount = request.Amount.ToDecimal2();

        var senderWallet = await _walletRepository.GetAll()
            .Include(t => t.Account)
            .SingleOrDefaultAsync(t => t.WalletNumber == request.SenderWalletNumber);

        if (senderWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.SenderWalletNumber);
        }

        var receiverWallet = await _walletRepository.GetAll()
            .Include(t => t.Account)
            .SingleOrDefaultAsync(t => t.WalletNumber == request.ReceiverWalletNumber);

        if (receiverWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.ReceiverWalletNumber);
        }

        ValidateWallets(senderWallet, receiverWallet);

        var pricingInfo = await CalculatePricingAsync(senderWallet, receiverWallet, request.Amount);
        var p2PCreditBalanceUsable = await GetP2PCreditBalanceUsableAsync();

        var isSameAccount = senderWallet.AccountId == receiverWallet.AccountId;

        if (!isSameAccount)
        {
            await ValidateDescriptionLengthAsync(request.PaymentType, request.Description);
            await _permissionService.ValidatePermissionAsync(senderWallet.Account.AccountKycLevel, TierPermissionType.P2PTransfer);
        }

        ValidateBalance(senderWallet, request.Amount, p2PCreditBalanceUsable, isSameAccount);

        await ValidateInternalTransferLimitsAsync(senderWallet, receiverWallet, pricingInfo.Amount);

        return new TransferPreviewResponse
        {
            Amount = pricingInfo.Amount,
            BsmvRate = pricingInfo.BsmvRate,
            BsmvTotal = pricingInfo.BsmvTotal,
            CommissionAmount = pricingInfo.CommissionAmount,
            CommissionRate = pricingInfo.CommissionRate,
            Fee = pricingInfo.Fee,
            Description = request.Description,
            ReceiverWalletNumber = request.ReceiverWalletNumber,
            SenderWalletNumber = request.SenderWalletNumber,
            ReceiverName = receiverWallet?.Account?.Name,
            PaymentType = request.PaymentType
        };
    }

    private async Task<CalculatePricingResponse> CalculatePricingAsync(Wallet senderWallet, Wallet receiverWallet, Decimal amount)
    {
        if (senderWallet.AccountId != receiverWallet.AccountId)
        {
            return await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
            {
                TransferType = TransferType.Internal,
                Amount = amount,
                BankCode = null,
                CurrencyCode = senderWallet.CurrencyCode,
                SenderWalletType = senderWallet.WalletType
            });
        }
        else
        {
            return new CalculatePricingResponse
            {
                Amount = amount,
                BsmvRate = 0,
                BsmvTotal = 0,
                CommissionAmount = 0,
                CommissionRate = 0,
                Fee = 0
            };
        }
    }

    private void ValidateWallets(Wallet senderWallet, Wallet receiverWallet)
    {
        ValidateStatus(senderWallet);

        ValidateStatus(receiverWallet);

        if (senderWallet.CurrencyCode != receiverWallet.CurrencyCode)
        {
            throw new CurrencyCodeMismatchException();
        }

        if (senderWallet.WalletNumber == receiverWallet.WalletNumber)
        {
            throw new InvalidWalletStatusException();
        }
    }

    private static void ValidateStatus(Wallet wallet)
    {
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException(wallet.WalletNumber);
        }
    }

    private static void ValidateBalance(Wallet wallet, decimal requestAmount, bool p2PCreditBalanceUsable = false, bool isSameAccount = false)
    {
        if (isSameAccount)
        {
            if (requestAmount > wallet.AvailableBalanceCash)
            {
                throw new SameAccountInsufficientBalanceException();
            }

            return;
        }

        if (!p2PCreditBalanceUsable)
        {
            if (requestAmount > wallet.AvailableBalanceCash)
            {
                throw new InsufficientBalanceException();
            }

            return;
        }

        if (requestAmount > wallet.AvailableBalance)
        {
            throw new InsufficientBalanceException();
        }
    }

    private async Task ValidateInternalTransferLimitsAsync(Wallet senderWallet, Wallet receiverWallet,
        decimal requestAmount)
    {
        if (senderWallet.AccountId == receiverWallet.AccountId)
        {
            return;
        }

        var senderTransferLimitControl = new LimitControlRequest
        {
            WalletNumber = senderWallet.WalletNumber,
            Amount = requestAmount,
            CurrencyCode = senderWallet.CurrencyCode,
            LimitOperationType = LimitOperationType.InternalTransfer,
            AccountId = senderWallet.AccountId
        };

        await CheckLimitAsync(senderTransferLimitControl);

        var receiverBalanceLimitControl = new LimitControlRequest
        {
            WalletNumber = receiverWallet.WalletNumber,
            Amount = requestAmount,
            CurrencyCode = receiverWallet.CurrencyCode,
            LimitOperationType = LimitOperationType.MaxBalance,
            AccountId = receiverWallet.AccountId
        };

        await CheckLimitAsync(receiverBalanceLimitControl);

        var receiverDepositLimitControl = new LimitControlRequest
        {
            WalletNumber = receiverWallet.WalletNumber,
            Amount = requestAmount,
            CurrencyCode = receiverWallet.CurrencyCode,
            LimitOperationType = LimitOperationType.Deposit,
            AccountId = receiverWallet.AccountId
        };

        await CheckLimitAsync(receiverDepositLimitControl);
    }

    private static void ValidateAccount(Wallet wallet, AccountUser accountUser)
    {
        if (wallet.AccountId != accountUser.AccountId)
        {
            throw new ForbiddenAccessException();
        }
    }

    private async Task<MoneyTransferResponse> ExecuteTransferAsync(Wallet senderWallet, Wallet receiverWallet, TransferCommand request,
         Account senderAccount, Account receiverAccount, CalculatePricingResponse pricing, bool isSameAccount)
    {
        try
        {
            var isGreaterThanMinAmountLimit =
                await _pricingCommercialService.IsGreaterThanMinAmountLimit(request.Amount);

            var senderTransactionId = Guid.Empty;
            var receiverTransactionId = Guid.Empty;

            CalculatePricingResponse receiverPricing = null;

            if (receiverAccount.IsCommercial && receiverWallet.AccountId != senderWallet.AccountId)
            {
                var commissionRates = await _pricingCommercialService.GetPricingCommercialRateAsync(receiverWallet.CurrencyCode, PricingCommercialType.P2P);

                if (commissionRates != null && receiverAccount.IsCommercial && isGreaterThanMinAmountLimit)
                {
                    receiverPricing =
                        await _pricingCommercialService.CalculateCustomPricingAsync(pricing.Amount, 0,
                            commissionRates.CommissionRate);
                }
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

            var strategy = new NoRetryExecutionStrategy(dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var wallets = await GetWalletsWithLockAsync(dbContext, senderWallet.Id, receiverWallet.Id);

                    var dbSenderWallet = wallets.FirstOrDefault(s => s.Id == senderWallet.Id);

                    NullControlHelper.CheckAndThrowIfNull(dbSenderWallet, senderWallet.Id, _logger);

                    var dbReceiverWallet = wallets.FirstOrDefault(s => s.Id == receiverWallet.Id);

                    NullControlHelper.CheckAndThrowIfNull(dbReceiverWallet, receiverWallet.Id, _logger);

                    bool p2PCreditBalanceUsable = await GetP2PCreditBalanceUsableAsync();

                    ValidateBalance(dbSenderWallet, pricing.TotalAmount, p2PCreditBalanceUsable, isSameAccount);

                    await ValidateInternalTransferLimitsAsync(dbSenderWallet, dbReceiverWallet, pricing.Amount);

                    dbContext.Attach(dbSenderWallet);

                    var senderTransaction = PopulateSenderTransaction(dbSenderWallet, request, receiverAccount,
                        senderAccount, dbReceiverWallet.Id);

                    dbContext.Transaction.Add(senderTransaction);

                    Withdraw(dbSenderWallet, pricing.Amount, p2PCreditBalanceUsable, isSameAccount);

                    if (pricing.PricingAmount > 0)
                    {
                        var senderPricingTransaction = PopulatePricingTransaction(dbSenderWallet, pricing, request.UserId,
                            PaymentMethod.Transfer, request.Description, senderTransaction.Id,
                            senderAccount, receiverAccount);

                        dbContext.Transaction.Add(senderPricingTransaction);

                        Withdraw(dbSenderWallet, pricing.PricingAmount, p2PCreditBalanceUsable, isSameAccount);

                        var senderBsmvTransaction = PopulateBsmvTransaction(dbSenderWallet, pricing, request.UserId,
                            PaymentMethod.Transfer, request.Description, senderTransaction.Id, senderAccount,
                            receiverAccount);

                        dbContext.Transaction.Add(senderBsmvTransaction);

                        Withdraw(dbSenderWallet, pricing.BsmvTotal, p2PCreditBalanceUsable, isSameAccount);
                    }

                    var receiverTransaction = PopulateReceiverTransaction(dbReceiverWallet, request, senderAccount,
                        receiverAccount, senderWallet.Id);

                    dbContext.Transaction.Add(receiverTransaction);

                    dbContext.Attach(dbReceiverWallet);

                    Deposit(dbReceiverWallet, request.Amount);

                    var receiverAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                        senderAccount, receiverAccount, TransactionDirection.MoneyIn, request.UserId);
                    dbContext.AccountActivity.Add(receiverAccountActivity);

                    var senderAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                        senderAccount, receiverAccount, TransactionDirection.MoneyOut, request.UserId);
                    dbContext.AccountActivity.Add(senderAccountActivity);

                    decimal commercialPricingTotal = 0;
                    if (dbSenderWallet.AccountId != dbReceiverWallet.AccountId)
                    {
                        if (receiverAccount.IsCommercial && isGreaterThanMinAmountLimit)
                        {
                            commercialPricingTotal = await ApplyCommercialPricingAsync(dbReceiverWallet, request, receiverTransaction, dbContext);
                        }
                    }

                    if (senderWallet.AccountId != receiverWallet.AccountId)
                    {
                        await IncreaseSenderTransferLimitUsageAsync(dbContext, senderWallet, request.Amount, Guid.Parse(request.UserId));

                        await IncreaseReceiverDepositLimitUsageAsync(dbContext, receiverWallet, request.Amount - commercialPricingTotal, Guid.Parse(request.UserId));
                    }

                    await dbContext.SaveChangesAsync();

                    senderTransactionId = senderTransaction.Id;
                    receiverTransactionId = receiverTransaction.Id;

                    await transactionScope.CommitAsync();

                    await SendAccountingQueueAsync(senderWallet, receiverWallet, pricing, receiverTransaction, receiverPricing, senderTransactionId);

                    var details = new Dictionary<string, string>
                          {
                              {"SenderWalletNumber", dbSenderWallet.WalletNumber.ToString() },
                              {"ReceiverWalletNumber", dbReceiverWallet.WalletNumber.ToString() },
                              {"TransactionId" ,receiverTransaction.Id.ToString() }
                          };

                    await SendP2PMoneyTransferAuditLogAsync(true, Guid.Parse(request.UserId), details);

                    _ = Task.Run(() =>
                        SendTransferBTransQueueAsync(dbSenderWallet, senderAccount, dbReceiverWallet,
                            receiverAccount, pricing, senderTransaction));

                }
                catch (Exception exception)
                {
                    _logger.LogError("Withdraw Transaction Error: {Exception}", exception);

                    await transactionScope.RollbackAsync();

                    throw;
                }
            });

            await _saveReceiptService.SendReceiptQueueAsync(receiverTransactionId);
            await _saveReceiptService.SendReceiptQueueAsync(senderTransactionId);

            if (!receiverAccount.IsCommercial)
            {
                await CheckAccountCurrentCommercialStatusAsync(receiverWallet, receiverAccount,
                    senderWallet.AccountId == receiverWallet.AccountId);
            }

            if (senderWallet.WalletType == WalletType.Individual && receiverWallet.WalletType == WalletType.Corporate)
            {
                var cashbackReq = new SendCashbackQueueRequest()
                {
                    TransactionId = senderTransactionId,
                    CorporateWalletNumber = receiverWallet.WalletNumber,
                    CorporateAccountName = receiverWallet.Account.Name
                };

                await _cashbackService.SendCashbackQueueAsync(cashbackReq);
            }

            await SendInformations(senderWallet.Id, receiverWallet.Id, senderAccount, receiverAccount, request.Amount);

            return new MoneyTransferResponse
            {
                Success = true,
                TransactionId = senderTransactionId
            };

        }
        catch (Exception exception)
        {
            if (exception is InsufficientBalanceException or LimitExceededException or CustomApiException)
            {
                throw;
            }

            _logger.LogError($"P2P Transfer Error : {exception}");

            var details = new Dictionary<string, string>
                       {
                            {"SenderWalletNumber", senderWallet.WalletNumber.ToString() },
                            {"ReceiverWalletNumber", receiverWallet.WalletNumber.ToString() },
                            {"ErrorMessage" , exception.Message}
                       };
            await SendP2PMoneyTransferAuditLogAsync(false, Guid.Parse(request.UserId), details);

            return new MoneyTransferResponse
            {
                Success = false,
                ErrorMessage = exception.Message
            };
        }
    }

    private async Task SendInformations(Guid senderWalletId, Guid receiverWalletId, Account senderAccount, Account receiverAccount, decimal amount)
    {
        try
        {
            if (receiverAccount.Id != senderAccount.Id)
            {
                var sendMail = await GetNotificationParamAsync("Email");

                var sendPush = await GetNotificationParamAsync("Push");

                if (sendPush || sendMail)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

                    var senderWallet = await dbContext.Wallet.FirstOrDefaultAsync(s => s.Id == senderWalletId);
                    var receiverWallet = await dbContext.Wallet.FirstOrDefaultAsync(s => s.Id == receiverWalletId);

                    var senderLimit = await _limitService.GetAccountLimitsQuery(new GetAccountLimitsQuery
                    {
                        AccountId = senderAccount.Id,
                        CurrencyCode = senderWallet.CurrencyCode
                    });

                    var senderRemainingLimit = senderLimit.InternalTransfer.MonthlyMaxAmount - senderLimit.InternalTransfer.MonthlyUserAmount;
                    var senderTemplate = new Dictionary<string, string>
                    {
                        { "walletName", senderWallet.FriendlyName },
                        { "receiver", receiverAccount.Name},
                        { "amount", amount.ToString("N2") },
                        { "limit", senderRemainingLimit.ToString("N2") },
                        { "availableBalance", senderWallet.AvailableBalance.ToString("N2") },
                        { "currentDate",DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };

                    var receiverLimit = await _limitService.GetAccountLimitsQuery(new GetAccountLimitsQuery
                    {
                        AccountId = receiverAccount.Id,
                        CurrencyCode = receiverWallet.CurrencyCode
                    });

                    var receiverRemainingLimit = receiverLimit.Deposit.MonthlyMaxAmount - receiverLimit.Deposit.MonthlyUserAmount;
                    var receiverTemplate = new Dictionary<string, string>
                    {
                        { "walletName", receiverWallet.FriendlyName },
                        { "sender", senderAccount.Name},
                        { "amount", amount.ToString("N2") },
                        { "limit", receiverRemainingLimit.ToString("N2") },
                        { "availableBalance", receiverWallet.AvailableBalance.ToString("N2") },
                        { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };

                    if (sendPush)
                    {
                        _ = Task.Run(() => SendP2PNotificationsAsync(senderAccount, receiverAccount, senderTemplate, receiverTemplate));
                    }

                    if (sendMail)
                    {
                        _ = Task.Run(() => SendP2PMailsAsync(senderAccount, receiverAccount, senderTemplate, receiverTemplate));
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendInformationException : {exception}");
        }
    }

    private async Task CheckAccountCurrentCommercialStatusAsync(Wallet receiverWallet, Account receiverAccount, bool ownAccount)
    {
        try
        {
            var isCommercialNow = await _pricingCommercialService.CheckIfAccountIsCommercialNowAsync(
                receiverWallet.CurrencyCode,
                PricingCommercialType.P2P, receiverAccount.Id, ownAccount);

            if (!receiverAccount.IsCommercial && isCommercialNow)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
                var dbReceiverAccount = await dbContext.Account
                    .FirstOrDefaultAsync(w =>
                        w.Id == receiverAccount.Id);

                dbReceiverAccount.IsCommercial = true;
                dbContext.Update(dbReceiverAccount);
                await dbContext.SaveChangesAsync();

                var pricingRate =
                    await _pricingCommercialService.GetPricingCommercialRateAsync(receiverWallet.CurrencyCode, PricingCommercialType.P2P);

                await _pricingCommercialService.SendCommercialInfoPushNotificationAsync(receiverAccount, pricingRate);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"CheckingCommercialException: {e}");
        }
    }

    private async Task<decimal> ApplyCommercialPricingAsync(Wallet receiverWallet, TransferCommand request, Transaction transaction, EmoneyDbContext dbContext)
    {
        var commercialPricing =
            await _pricingCommercialService.GetPricingCommercialRateAsync(receiverWallet.CurrencyCode,
                PricingCommercialType.P2P);

        if (commercialPricing is null)
        {
            _logger.LogInformation($"Account {receiverWallet.AccountId} is commercial but there is no commercial pricing rate defined!");
            return 0;
        }

        var customPricing = await _pricingCommercialService.CalculateCustomPricingAsync(request.Amount,
            decimal.Zero, commercialPricing.CommissionRate);

        var pricingTransaction = PopulateCommercialUsageTransaction(receiverWallet, customPricing, transaction.Id, request);
        dbContext.Transaction.Add(pricingTransaction);
        Withdraw(receiverWallet, customPricing.CommissionAmount);

        var bsmvTransaction = PopulateCommercialUsageBsmvTransaction(receiverWallet, customPricing, PaymentMethod.Transfer, transaction.Id, request);
        dbContext.Transaction.Add(bsmvTransaction);
        Withdraw(receiverWallet, customPricing.BsmvTotal);
        return customPricing.CommissionAmount + customPricing.BsmvTotal;
    }

    private Transaction PopulateCommercialUsageBsmvTransaction
    (
        Wallet wallet,
        CalculatePricingResponse pricing,
        PaymentMethod paymentMethod,
        Guid relatedTransactionId,
        TransferCommand request)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = paymentMethod == PaymentMethod.Transfer
                ? TransactionStatus.Completed
                : TransactionStatus.Pending,
            Tag = _tagLocalizer.GetString($"CommercialUsagePrice"),
            TagTitle = TransactionType.Tax.ToString(),
            Amount = pricing.BsmvTotal,
            CurrencyCode = wallet.CurrencyCode,
            Description = _tagLocalizer.GetString($"CommercialUsagePrice"),
            WalletId = wallet.Id,
            CreatedBy = request.UserId,
            CurrentBalance = wallet.AvailableBalance - pricing.BsmvTotal,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = string.Empty,
            SenderName = string.Empty,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private async Task SendP2PNotificationsAsync(Account senderAccount, Account receiverAccount,
        Dictionary<string, string> senderTemplate,
        Dictionary<string, string> receiverTemplate)
    {
        await PushSenderNotificationAsync(senderTemplate, senderAccount);
        await PushReceiverNotificationsAsync(receiverTemplate, receiverAccount);
    }

    private async Task PushReceiverNotificationsAsync(Dictionary<string, string> templateData, Account receiverAccount)
    {
        var receiverUserIdList = receiverAccount.AccountUsers
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
        var receiverUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = receiverUserIdList.Select(x => x.UserId).ToList(),
        });
        var receiverNotificationRequest = new SendPushNotification
        {
            TemplateName = "MTDepositP2P",
            TemplateParameters = templateData,
            Tokens = receiverUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = receiverUserIdList
        };

        await _pusNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
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
            TemplateName = "MTWithdrawP2P",
            TemplateParameters = templateData,
            Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = senderUserList
        };

        await _pusNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
    }

    private async Task SendP2PMailsAsync(Account senderAccount, Account receiverAccount,
        Dictionary<string, string> senderTemplate, Dictionary<string, string> receiverTemplate)
    {
        await SendInformationMailSenderAsync(senderAccount, senderTemplate);
        await SendInformationMailReceiverAsync(receiverAccount, receiverTemplate);
    }

    private async Task SendInformationMailReceiverAsync(Account account, Dictionary<string, string> templateData)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "MTDepositP2P",
            DynamicTemplateData = templateData,
            ToEmail = account.Email
        };

        await _emailSender.SendEmailAsync(mailParams);
    }

    private async Task SendInformationMailSenderAsync(Account account, Dictionary<string, string> templateData)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "MTWithdrawP2P",
            DynamicTemplateData = templateData,
            ToEmail = account.Email
        };

        await _emailSender.SendEmailAsync(mailParams);
    }

    private async Task SendP2PMoneyTransferAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = isSuccess,
                   LogDate = DateTime.Now,
                   Operation = "P2PMoneyTransfer",
                   SourceApplication = "Emoney",
                   Resource = "MoneyTransfer",
                   UserId = userId,
                   Details = details
               }
           );
    }

    private async Task SendAccountingQueueAsync(Wallet senderWallet, Wallet receiverWallet,
                                                CalculatePricingResponse pricing,
                                                Transaction receiverTransaction,
                                                CalculatePricingResponse receiverPricing,
                                                Guid senderTransactionId)
    {
        AccountingPayment payment = new AccountingPayment
        {
            Amount = receiverTransaction.Amount,
            BsmvAmount = pricing.BsmvTotal,
            CommissionAmount = pricing.PricingAmount,
            CurrencyCode = senderWallet.CurrencyCode,
            Destination = $"WA-{receiverWallet.WalletNumber}",
            HasCommission = pricing.PricingAmount > 0,
            OperationType = OperationType.EmoneyTransfer,
            Source = $"WA-{senderWallet.WalletNumber}",
            TransactionDate = receiverTransaction.TransactionDate,
            UserId = Guid.Empty,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            AccountingTransactionType = AccountingTransactionType.Emoney,
            TransactionId = senderTransactionId
        };

        if (receiverPricing is not null && receiverPricing.CommissionAmount > 0)
        {
            payment.ReceiverCommissionAmount = receiverPricing.CommissionAmount;
            payment.ReceiverBsmvAmount = receiverPricing.BsmvTotal;
        }

        await _accountingService.SavePaymentAsync(payment);
    }

    private async Task SendTransferBTransQueueAsync(Wallet senderWallet, Account senderAccount,
                                                    Wallet receiverWallet, Account receiverAccount,
                                                    CalculatePricingResponse pricing, Transaction transaction)
    {
        try
        {
            var totalPricingAmount = pricing.PricingAmount + pricing.BsmvTotal + transaction.Amount;

            #region MoneyTransferReport
            var senderBTransIdentity = _bTransService.GetAccountInformation(senderAccount);
            var receiverBTransIdentity = _bTransService.GetAccountInformation(receiverAccount);
            var moneyTransfer = new MoneyTransferReport
            {
                RecordType = RecordTypeConst.NewRecord,
                OperationType = BTransOperationType.AccountToAccount,
                TransferType = BTransTransferType.AccountToAccount,

                //SenderBlock
                IsSenderCustomer = true,
                IsSenderCorporate = senderBTransIdentity.IsCorporate,
                SenderPhoneNumber = senderBTransIdentity.PhoneNumber,
                SenderEmail = senderBTransIdentity.Email,
                SenderWalletNumber = senderWallet.WalletNumber,
                SenderCityId = 0,
                SenderTaxNumber = senderBTransIdentity.TaxNumber,
                SenderCommercialTitle = senderBTransIdentity.CommercialTitle,
                SenderFirstName = senderBTransIdentity.FirstName,
                SenderLastName = senderBTransIdentity.LastName,
                SenderIdentityNumber = senderBTransIdentity.IdentityNumber,

                //ReceiverBlock
                IsReceiverCustomer = true,
                IsReceiverCorporate = receiverBTransIdentity.IsCorporate,
                ReceiverPhoneNumber = receiverBTransIdentity.PhoneNumber,
                ReceiverEmail = receiverBTransIdentity.Email,
                ReceiverWalletNumber = receiverWallet.WalletNumber,
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
                TotalPricingAmount = totalPricingAmount,
                TransferReason = BTransTransferReason.Other,
                IpAddress = _contextProvider.CurrentContext.ClientIpAddress,
                CustomerDescription = transaction.Description,
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
            _logger.LogError(exception, $"Failed to send transfer transaction [{transaction.Id}] to BTrans reporting tool  Error : {exception}");
        }
    }

    private async Task IncreaseWithdrawLimitUsageAsync(
        EmoneyDbContext emoneyDbContext, Wallet senderWallet, decimal amount, Guid userId, bool isOwnIban, string iban)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == senderWallet.AccountId
                                       && x.CurrencyCode == senderWallet.CurrencyCode);
        var isDailyDistinctIban = false;
        var isMonthlyDistinctIban = false;
        if (!isOwnIban)
        {
            isDailyDistinctIban =
                await _accountActivityService.IsWithdrawIbanDistinctAsync(
                    senderWallet.AccountId, TimeInterval.Daily, iban);
            isMonthlyDistinctIban =
                await _accountActivityService.IsWithdrawIbanDistinctAsync(
                    senderWallet.AccountId, TimeInterval.Monthly, iban);
        }
        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(senderWallet.CurrencyCode, senderWallet.AccountId, userId);
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.Withdrawal,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType,
                IsOwnIban = isOwnIban,
                IsDailyDistinctIban = isDailyDistinctIban,
                IsMonthlyDistinctIban = isMonthlyDistinctIban
            }, level);
            emoneyDbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.Withdrawal,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType,
                IsOwnIban = isOwnIban,
                IsDailyDistinctIban = isDailyDistinctIban,
                IsMonthlyDistinctIban = isMonthlyDistinctIban
            }, existingLevel);
            emoneyDbContext.Update(existingLevel);
        }
    }

    private async Task IncreaseSenderTransferLimitUsageAsync(EmoneyDbContext emoneyDbContext, Wallet senderWallet, decimal amount, Guid userId)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == senderWallet.AccountId
                                       && x.CurrencyCode == senderWallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(senderWallet.CurrencyCode, senderWallet.AccountId, userId);
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.InternalTransfer,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType
            }, level);
            emoneyDbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.InternalTransfer,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType
            }, existingLevel);
            emoneyDbContext.Update(existingLevel);
        }
    }

    private async Task IncreaseReceiverDepositLimitUsageAsync(EmoneyDbContext emoneyDbContext, Wallet receiverWallet, decimal amount, Guid userId)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == receiverWallet.AccountId
                                       && x.CurrencyCode == receiverWallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(receiverWallet.CurrencyCode, receiverWallet.AccountId, userId);
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = receiverWallet.AccountId,
                LimitOperationType = LimitOperationType.Deposit,
                Amount = amount,
                CurrencyCode = receiverWallet.CurrencyCode,
                WalletType = receiverWallet.WalletType
            }, level);
            emoneyDbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = receiverWallet.AccountId,
                LimitOperationType = LimitOperationType.Deposit,
                Amount = amount,
                CurrencyCode = receiverWallet.CurrencyCode,
                WalletType = receiverWallet.WalletType
            }, existingLevel);
            emoneyDbContext.Update(existingLevel);
        }
    }

    private AccountActivity PopulateAccountTransfer(Wallet senderWallet, Wallet receiverWallet,
        decimal amount, Account senderAccount, Account receiverAccount, TransactionDirection transactionDirection, string createdBy)
    {
        return new AccountActivity
        {
            Id = Guid.NewGuid(),
            AccountId = transactionDirection == TransactionDirection.MoneyIn ? receiverAccount.Id : senderAccount.Id,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy,
            RecordStatus = RecordStatus.Active,
            TransferType = PricingCommercialType.P2P.ToString(),
            Sender = $"WA-{senderWallet.WalletNumber}",
            TransactionDirection = transactionDirection,
            Receiver = $"WA-{receiverWallet.WalletNumber}",
            Amount = amount,
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            OwnAccount = senderWallet.AccountId == receiverWallet.AccountId
        };
    }

    private Transaction PopulateSenderTransaction(Wallet senderWallet, TransferCommand request,
        Account receiverAccount, Account senderAccount, Guid receiverWalletId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Withdraw,
            TransactionStatus = TransactionStatus.Completed,
            Tag = receiverAccount.Name,
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = request.Amount,
            CurrencyCode = senderWallet.CurrencyCode,
            Description = request.Description,
            WalletId = senderWallet.Id,
            CreatedBy = request.UserId,
            CurrentBalance = senderWallet.AvailableBalance - request.Amount,
            PreBalance = senderWallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Transfer,
            RecordStatus = RecordStatus.Active,
            ReceiverName = receiverAccount.Name,
            SenderName = senderAccount.Name,
            CounterWalletId = receiverWalletId,
            Channel = _contextProvider.CurrentContext.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            PaymentType = request.PaymentType,
            IdempotentKey = request.IdempotentKey,
        };
    }

    private Transaction PopulatePricingTransaction(Wallet wallet, CalculatePricingResponse pricing,
        string userId, PaymentMethod paymentMethod, string description, Guid relatedTransactionId,
        Account senderAccount = null, Account receiverAccount = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = paymentMethod == PaymentMethod.Transfer
                ? TransactionStatus.Completed
                : TransactionStatus.Pending,
            Tag = receiverAccount != null
            ? string.Concat(receiverAccount.Name, " - ", _tagLocalizer.GetString(TransactionType.Commission.ToString()))
            : _tagLocalizer.GetString(TransactionType.Commission.ToString()),
            TagTitle = TransactionType.Commission.ToString(),
            Amount = pricing.PricingAmount,
            CurrencyCode = wallet.CurrencyCode,
            Description = description,
            WalletId = wallet.Id,
            CreatedBy = userId,
            CurrentBalance = wallet.AvailableBalance - pricing.PricingAmount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = receiverAccount != null ? receiverAccount.Name : null,
            SenderName = senderAccount != null ? senderAccount.Name : null,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulateBsmvTransaction(Wallet wallet, CalculatePricingResponse pricing, string userId,
        PaymentMethod paymentMethod, string description, Guid relatedTransactionId,
        Account senderAccount = null, Account receiverAccount = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = paymentMethod == PaymentMethod.Transfer
                ? TransactionStatus.Completed
                : TransactionStatus.Pending,
            Tag = receiverAccount != null
            ? string.Concat(receiverAccount.Name, " - ", _tagLocalizer.GetString(TransactionType.Tax.ToString()))
            : _tagLocalizer.GetString(TransactionType.Tax.ToString()),
            TagTitle = TransactionType.Tax.ToString(),
            Amount = pricing.BsmvTotal,
            CurrencyCode = wallet.CurrencyCode,
            Description = description,
            WalletId = wallet.Id,
            CreatedBy = userId,
            CurrentBalance = wallet.AvailableBalance - pricing.BsmvTotal,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            ReceiverName = receiverAccount != null ? receiverAccount.Name : null,
            SenderName = senderAccount?.Name,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulateReceiverTransaction(Wallet receiverWallet, TransferCommand request,
        Account senderAccount, Account receiverAccount, Guid senderWalletId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Deposit,
            TransactionStatus = TransactionStatus.Completed,
            Tag = senderAccount.Name,
            TagTitle = TransactionType.Deposit.ToString(),
            Amount = request.Amount,
            CurrencyCode = receiverWallet.CurrencyCode,
            Description = request.Description,
            WalletId = receiverWallet.Id,
            CreatedBy = request.UserId,
            CurrentBalance = receiverWallet.AvailableBalance + request.Amount,
            PreBalance = receiverWallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Transfer,
            RecordStatus = RecordStatus.Active,
            SenderName = senderAccount.Name,
            ReceiverName = receiverAccount.Name,
            CounterWalletId = senderWalletId,
            Channel = _contextProvider.CurrentContext?.Channel,
            PaymentType = request.PaymentType,
            IdempotentKey = request.IdempotentKey,
        };
    }

    private Transaction PopulateCommercialUsageTransaction(Wallet wallet, CalculatePricingResponse customPricing,
        Guid relatedTransactionId, TransferCommand request)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = TransactionStatus.Completed,
            Tag = _tagLocalizer.GetString($"CommercialUsagePrice"),
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = customPricing.CommissionAmount,
            CurrencyCode = wallet.CurrencyCode,
            Description = _tagLocalizer.GetString($"CommercialUsagePrice"),
            WalletId = wallet.Id,
            CreatedBy = request.UserId,
            CurrentBalance = wallet.AvailableBalance - customPricing.CommissionAmount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Transfer,
            RecordStatus = RecordStatus.Active,
            ReceiverBankCode = null,
            ReceiverName = string.Empty,
            Channel = _contextProvider.CurrentContext?.Channel,
            SenderName = string.Empty,
            RelatedTransactionId = relatedTransactionId,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulateWithdrawTransaction(Wallet wallet, WithdrawRequestCommand request,
        int ibanBankCode, Account senderAccount)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Withdraw,
            TransactionStatus = TransactionStatus.Pending,
            Tag = request.ReceiverName,
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = request.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = request.Description,
            WalletId = wallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = wallet.AvailableBalance - request.Amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.BankTransfer,
            RecordStatus = RecordStatus.Active,
            ReceiverBankCode = ibanBankCode,
            ReceiverName = request.ReceiverName,
            Channel = _contextProvider.CurrentContext?.Channel,
            SenderName = senderAccount.Name,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            PaymentType = request.PaymentType,
            IdempotentKey = request.IdempotentKey,
        };
    }

    private static WithdrawRequest PopulateWithdrawRequest(Guid transactionId, WithdrawRequestCommand request,
        string currencyCode, MoneyTransferModel moneyTransferInfo, bool isOwnIban)
    {
        return new WithdrawRequest
        {
            Amount = request.Amount,
            WalletNumber = request.WalletNumber,
            Description = request.Description,
            ReceiverIbanNumber = request.ReceiverIBAN,
            ReceiverName = request.ReceiverName,
            CreatedBy = request.UserId.ToString(),
            InternalTransactionId = transactionId,
            WithdrawStatus = WithdrawStatus.Pending,
            CurrencyCode = currencyCode,
            RecordStatus = RecordStatus.Active,
            TransactionBankCode = moneyTransferInfo.TransferBankCode,
            TransferType = moneyTransferInfo.TransferType,
            ReceiverBankCode = moneyTransferInfo.IbanBankCode,
            TransactionBankName = moneyTransferInfo.TransferBankName,
            ReceiverBankName = moneyTransferInfo.IbanBankName,
            IsReceiverIbanOwned = isOwnIban
        };
    }

    private void Withdraw(Wallet wallet, decimal amount, bool p2PCreditBalanceUsable = false, bool isSameAccount = false)
    {
        if (isSameAccount)
        {
            wallet.CurrentBalanceCash -= amount;
        }
        else
        {
            if (p2PCreditBalanceUsable)
            {
                if (amount <= wallet.AvailableBalanceCredit)
                {
                    wallet.CurrentBalanceCredit -= amount;
                }
                else
                {
                    var creditToUse = wallet.AvailableBalanceCredit;
                    var difference = amount - creditToUse;

                    if (difference > wallet.AvailableBalanceCash)
                    {
                        throw new InsufficientBalanceException();
                    }

                    wallet.CurrentBalanceCredit -= creditToUse;
                    wallet.CurrentBalanceCash -= difference;
                }
            }
            else
            {
                wallet.CurrentBalanceCash -= amount;
            }
        }
    }

    private void Deposit(Wallet wallet, decimal amount)
    {
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        wallet.CurrentBalanceCash += amount;
        wallet.LastActivityDate = DateTime.Now;
    }

    public async Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequestCommand request, CancellationToken cancellationToken)
    {
        await ValidateDescriptionLengthAsync(request.PaymentType, request.Description);

        using var scope = _scopeFactory.CreateScope();

        var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var wallet = _dbContext.Wallet.SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.WalletNumber);
        }

        var senderAccountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        var senderAccount = senderAccountUser.Account;

        ValidateAccount(wallet, senderAccountUser);

        ValidateStatus(wallet);

        var ibanValidationResult = await _moneyTransferService.CheckIbanAsync(request.ReceiverIBAN);

        if (!ibanValidationResult.IsValidIban)
        {
            throw new InvalidIbanException();
        }

        await CheckDuplicateTransactionAsync(request.IdempotentKey);

        var kkbResult = await _accountIbanService.ValidateIbanAsync(senderAccount.IdentityNumber, request.ReceiverIBAN);

        await ValidateWithdrawPermissionAsync(senderAccount, kkbResult);

        request.Amount = request.Amount.ToDecimal2();

        var transferBank = await _moneyTransferService.GetTransferBankAsync(new GetTransferBankRequest
        {
            Amount = request.Amount,
            ReceiverIBAN = request.ReceiverIBAN,
            CurrencyCode = wallet.CurrencyCode,
        });

        if (transferBank.TransferType.ToString() == TransferType.Eft.ToString() &&
                    !transferBank.IsEftSuitableNow)
        {
            throw new MoneyTransferOutsideEftHoursException();
        }

        var pricingInfo = await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
        {
            TransferType = (TransferType)transferBank.TransferType,
            Amount = request.Amount,
            BankCode = transferBank.TransferBankCode,
            CurrencyCode = wallet.CurrencyCode,
            SenderWalletType = wallet.WalletType
        });

        var moneyTransferInfo = new MoneyTransferModel
        {
            TransferBankCode = transferBank.TransferBankCode,
            TransferBankName = transferBank.TransferBankName,
            TransferType = transferBank.TransferType,
            IbanBankCode = transferBank.IbanBankCode,
            IbanBankName = transferBank.IbanBankName,
        };

        var currencyCode = await GetCurrencyCodeAsync(wallet.CurrencyCode);

        var IsTransactionCheckEnabled =
          _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "TransactionEnabled");

        if (IsTransactionCheckEnabled)
        {
            var requestModel = new FraudTransactionDetail
            {
                Amount = request.Amount,
                BeneficiaryNumber = request.ReceiverIBAN,
                Beneficiary = request.ReceiverName,
                OriginatorNumber = request.WalletNumber,
                Originator = senderAccount.Name,
                FraudSource = FraudSource.Wallet,
                Direction = Direction.Outbound,
                AmountCurrencyCode = Convert.ToInt32(currencyCode),
                BeneficiaryAccountCurrencyCode = Convert.ToInt32(currencyCode),
                OriginatorAccountCurrencyCode = Convert.ToInt32(currencyCode),
                Channel = _contextProvider.CurrentContext.Channel
            };

            await CheckFraudAsync(new FraudCheckRequest
            {
                CommandName = "Withdraw",
                ExecuteTransaction = requestModel,
                UserId = request.UserId.ToString(),
                Module = "Emoney",
                AccountKycLevel = senderAccount.AccountKycLevel,
                CommandJson = JsonConvert.SerializeObject(requestModel)
            });
        }

        return await ExecuteWithdrawAsync(wallet, request, transferBank.IbanBankCode, moneyTransferInfo, pricingInfo, senderAccount, kkbResult);
    }

    private async Task ValidateWithdrawLimitAsync(Account senderAccount, Wallet wallet, decimal requestAmount)
    {
        var limitParameter = new ParameterDto();

        try
        {
            limitParameter = await _parameterService.GetParameterAsync("EmoneyTransferParameters", NoneKYCWithdrawalLimit);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error validating NoneKYC withdrawal limit: {Exception}", ex);
        }

        var isNoneKyc = senderAccount.AccountKycLevel == AccountKycLevel.NoneKyc;
        
        if (!string.IsNullOrEmpty(limitParameter.ParameterValue) && isNoneKyc)
        {
            var startOfYear = new DateTime(DateTime.Now.Year, 1, 1);

            var transactionSum = await _transactionRepository.GetAll()
                .Where(t => t.WalletId == wallet.Id
                            && t.TransactionType == TransactionType.Withdraw
                            && t.PaymentMethod == PaymentMethod.BankTransfer
                            && t.TransactionDate.Date >= startOfYear
                            && t.TransactionDate.Date <= DateTime.Now.Date
                            && (t.TransactionStatus == TransactionStatus.Completed || t.TransactionStatus == TransactionStatus.Pending)
                            && t.RecordStatus == RecordStatus.Active
                            && t.IsReturned == false)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            if (transactionSum + requestAmount > decimal.Parse(limitParameter.ParameterValue))
            {
                throw new CustomApiException(ApiErrorCode.LimitExceeded,
                    _exceptionLocalizer.GetString($"{LimitOperationType.Withdrawal}LimitExceededException"));
            }
        }

        var limitControlRequest = new LimitControlRequest
        {
            WalletNumber = wallet.WalletNumber,
            CurrencyCode = wallet.CurrencyCode,
            Amount = requestAmount,
            LimitOperationType = LimitOperationType.Withdrawal,
            AccountId = wallet.AccountId
        };

        await CheckLimitAsync(limitControlRequest);
    }

    private async Task ValidateWithdrawIbanLimitAsync(Wallet wallet, decimal requestAmount, bool isOwnIban, string iban)
    {
        var isDailyDistinctIban = false;
        var isMonthlyDistinctIban = false;
        if (!isOwnIban)
        {
            isDailyDistinctIban =
                await _accountActivityService.IsWithdrawIbanDistinctAsync(
                    wallet.AccountId, TimeInterval.Daily, iban);
            isMonthlyDistinctIban =
                await _accountActivityService.IsWithdrawIbanDistinctAsync(
                    wallet.AccountId, TimeInterval.Monthly, iban);
        }

        var limitControlRequest = new LimitControlRequest
        {
            WalletNumber = wallet.WalletNumber,
            CurrencyCode = wallet.CurrencyCode,
            Amount = requestAmount,
            LimitOperationType = LimitOperationType.WithdrawIban,
            AccountId = wallet.AccountId,
            IsOwnIban = isOwnIban,
            IsDailyDistinctIban = isDailyDistinctIban,
            IsMonthlyDistinctIban = isMonthlyDistinctIban
        };
        await CheckLimitAsync(limitControlRequest);
    }

    private async Task CheckLimitAsync(LimitControlRequest limitControlRequest)
    {
        var limitResponse = await _limitService.IsLimitExceededAsync(limitControlRequest);

        if (limitResponse.IsLimitExceeded)
        {
            throw new CustomApiException(ApiErrorCode.LimitExceeded,
                _exceptionLocalizer.GetString($"{limitResponse.LimitOperationType}LimitExceededException"));
        }
    }

    private async Task<MoneyTransferResponse> ExecuteWithdrawAsync(Wallet wallet, WithdrawRequestCommand request, int ibanBankCode,
        MoneyTransferModel moneyTransferInfo, CalculatePricingResponse pricing, Account senderAccount, bool kkbResult)
    {
        WithdrawRequest withdrawRequest = new();

        try
        {
            request.Amount = request.Amount.ToDecimal2();

            var transactionId = Guid.Empty;

            using var scope = _scopeFactory.CreateScope();

            var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

            var strategy = new NoRetryExecutionStrategy(_dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var dbWallet = await GetWalletWithLockAsync(_dbContext, wallet.Id);

                    _dbContext.Attach(dbWallet);

                    ValidateBalance(dbWallet, pricing.TotalAmount);

                    await ValidateWithdrawLimitAsync(senderAccount, dbWallet, pricing.Amount);

                    await ValidateWithdrawIbanLimitAsync(wallet, pricing.Amount, kkbResult, request.ReceiverIBAN);

                    var transaction = PopulateWithdrawTransaction(dbWallet, request, ibanBankCode, senderAccount);
                    _dbContext.Transaction.Add(transaction);

                    Withdraw(dbWallet, request.Amount);

                    if (pricing.PricingAmount > 0)
                    {
                        var pricingTransaction = PopulatePricingTransaction(dbWallet, pricing,
                            request.UserId.ToString(),
                            PaymentMethod.BankTransfer, request.Description, transaction.Id, senderAccount);
                        _dbContext.Transaction.Add(pricingTransaction);

                        Withdraw(dbWallet, pricing.PricingAmount);

                        var bsmvTransaction = PopulateBsmvTransaction(dbWallet, pricing, request.UserId.ToString(),
                            PaymentMethod.BankTransfer, request.Description, transaction.Id, senderAccount);
                        _dbContext.Transaction.Add(bsmvTransaction);

                        Withdraw(dbWallet, pricing.BsmvTotal);
                    }

                    withdrawRequest = PopulateWithdrawRequest(transaction.Id, request, dbWallet.CurrencyCode,
                        moneyTransferInfo, kkbResult);
                    _dbContext.WithdrawRequest.Add(withdrawRequest);

                    transaction.WithdrawRequestId = withdrawRequest.Id;

                    await IncreaseWithdrawLimitUsageAsync(_dbContext, wallet, request.Amount, request.UserId, kkbResult,
                        request.ReceiverIBAN);

                    await _dbContext.SaveChangesAsync();

                    transactionId = transaction.Id;

                    await transactionScope.CommitAsync();

                }
                catch (Exception exception)
                {
                    _logger.LogError("Withdraw Transaction Error: {Exception}", exception);

                    await transactionScope.RollbackAsync();

                    throw;
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "ExecuteWithdraw",
                        SourceApplication = "Emoney",
                        Resource = "WithdrawRequest",
                        Details = new Dictionary<string, string>
                        {
                            {"Id", withdrawRequest.Id.ToString() },
                            {"WalletNumber", withdrawRequest.WalletNumber },
                            {"ReceiverIbanNumber", withdrawRequest.ReceiverIbanNumber },
                            {"Amount", withdrawRequest.Amount.ToString() }
                        }
                    });
            });

            await _saveReceiptService.SendReceiptQueueAsync(transactionId);

            return new MoneyTransferResponse
            {
                Success = true,
                TransactionId = transactionId
            };
        }
        catch (Exception exception)
        {
            if (exception is InsufficientBalanceException or LimitExceededException)
            {
                throw;
            }

            _logger.LogError($"Withdraw Request Error : {exception}");

            return new MoneyTransferResponse
            {
                Success = false,
                ErrorMessage = exception.Message
            };
        }
    }

    public async Task<WithdrawPreviewResponse> WithdrawPreviewAsync(WithdrawPreviewQuery request)
    {
        await ValidateDescriptionLengthAsync(request.PaymentType, request.Description);
        using var scope = _scopeFactory.CreateScope();

        var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var wallet = _dbContext.Wallet.SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet), request.WalletNumber);
        }

        var senderAccountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        var senderAccount = senderAccountUser.Account;

        ValidateAccount(wallet, senderAccountUser);

        ValidateStatus(wallet);

        var ibanValidationResult = await _moneyTransferService.CheckIbanAsync(request.ReceiverIBAN);

        if (!ibanValidationResult.IsValidIban)
        {
            throw new InvalidIbanException();
        }

        var kkbResult = await _accountIbanService.ValidateIbanAsync(senderAccount.IdentityNumber, request.ReceiverIBAN);

        await ValidateWithdrawPermissionAsync(senderAccount, kkbResult);

        request.Amount = request.Amount.ToDecimal2();

        var transferBank = await _moneyTransferService.GetTransferBankAsync(new GetTransferBankRequest
        {
            Amount = request.Amount,
            ReceiverIBAN = request.ReceiverIBAN,
            CurrencyCode = wallet.CurrencyCode,
        });

        if (transferBank.TransferType.ToString() == TransferType.Eft.ToString() &&
            !transferBank.IsEftSuitableNow)
        {
            throw new MoneyTransferOutsideEftHoursException();
        }

        var pricingInfo = await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
        {
            TransferType = (TransferType)transferBank.TransferType,
            Amount = request.Amount,
            BankCode = transferBank.TransferBankCode,
            CurrencyCode = wallet.CurrencyCode,
            SenderWalletType = wallet.WalletType
        });

        ValidateBalance(wallet, pricingInfo.TotalAmount);

        await ValidateWithdrawLimitAsync(senderAccount, wallet, pricingInfo.Amount);

        await ValidateWithdrawIbanLimitAsync(wallet, pricingInfo.Amount, kkbResult, request.ReceiverIBAN);

        return new WithdrawPreviewResponse
        {
            Amount = pricingInfo.Amount,
            BsmvRate = pricingInfo.BsmvRate,
            BsmvTotal = pricingInfo.BsmvTotal,
            CommissionAmount = pricingInfo.CommissionAmount,
            CommissionRate = pricingInfo.CommissionRate,
            Fee = pricingInfo.Fee,
            ReceiverIBAN = request.ReceiverIBAN,
            ReceiverName = request.ReceiverName,
            Description = request.Description,
            WalletNumber = request.WalletNumber,
            PaymentType = request.PaymentType
        };
    }

    private async Task<Wallet> GetWalletWithLockAsync(EmoneyDbContext dbContext, Guid senderWalletId)
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
                                    "AND RecordStatus = 'Active'", senderWalletId)
                        .FirstOrDefaultAsync();
                }
            default:
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM core.wallet " +
                                    "WHERE id = {0} " +
                                    "AND record_status = 'Active' FOR UPDATE", senderWalletId)
                        .FirstOrDefaultAsync();
                }
        }
    }

    private async Task<List<Wallet>> GetWalletsWithLockAsync(EmoneyDbContext dbContext, Guid senderWalletId, Guid receiverWalletId)
    {
        var databaseProvider = await _databaseProviderService.GetProviderAsync();
        switch (databaseProvider)
        {
            case "MsSql":
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM Core.Wallet WITH(ROWLOCK, UPDLOCK) " +
                                    "WHERE Id IN ({0}, {1}) " +
                                    "AND RecordStatus = 'Active'", senderWalletId, receiverWalletId)
                        .ToListAsync();
                }
            default:
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM core.wallet " +
                                    "WHERE id IN ({0},{1}) " +
                                    "AND record_status = 'Active' FOR UPDATE", senderWalletId, receiverWalletId)
                        .ToListAsync();
                }
        }
    }

    private async Task ValidateWithdrawPermissionAsync(Account account, bool kkbResult)
    {
        if (kkbResult)
        {
            await _permissionService.ValidatePermissionAsync(account.AccountKycLevel, TierPermissionType.WithdrawToOwnIban);
        }
        else
        {
            await _permissionService.ValidatePermissionAsync(account.AccountKycLevel, TierPermissionType.WithdrawToOtherIban);
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

    public async Task ValidateCurrentAndSenderUser(string senderWalletNumber)
    {
        var senderAccount = await _walletRepository
            .GetAll()
            .Select(s => new
            {
                s.AccountId,
                s.WalletNumber
            })
            .Where(s => s.WalletNumber == senderWalletNumber)
            .FirstOrDefaultAsync();

        var senderUserIdList = await _accountUserRepository
            .GetAll()
            .Select(s => new
            {
                s.AccountId,
                s.UserId
            })
            .Where(s => s.AccountId == senderAccount.AccountId)
            .ToListAsync();

        if (_contextProvider.CurrentContext.UserId is null ||
            !senderUserIdList.Any(s => s.UserId == Guid.Parse(_contextProvider.CurrentContext.UserId)))
        {
            throw new ForbiddenAccessException();
        }

    }

    private async Task<bool> GetP2PCreditBalanceUsableAsync()
    {
        bool p2PCreditBalanceUsable = true;

        try
        {
            var parameter = await _parameterService.GetParameterAsync("EmoneyTransferParameters", "P2PCreditBalanceUsable");
            p2PCreditBalanceUsable = Convert.ToBoolean(parameter.ParameterValue);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Parameter : {exception}");
        }

        return p2PCreditBalanceUsable;
    }

    public async Task ValidateUserAndSenderUser(string senderWalletNumber, Guid userId)
    {
        var senderAccount = await _walletRepository
           .GetAll()
           .Select(s => new
           {
               s.AccountId,
               s.WalletNumber
           })
           .Where(s => s.WalletNumber == senderWalletNumber)
           .FirstOrDefaultAsync();

        var senderUserIdList = await _accountUserRepository
            .GetAll()
            .Select(s => new
            {
                s.AccountId,
                s.UserId
            })
            .Where(s => s.AccountId == senderAccount.AccountId)
            .ToListAsync();

        if (!senderUserIdList.Any(s => s.UserId == userId))
        {
            throw new ForbiddenAccessException();
        }
    }

    public async Task ValidateDescriptionLengthAsync(string paymentType, string description)
    {
        description ??= string.Empty;
        var parameters = await _parameterService.GetParametersAsync(MoneyTransferParameterGroup);

        var parameter = parameters.FirstOrDefault(p => p.ParameterValue == paymentType);

        if (parameter is null)
        {
            throw new NotFoundException($"PaymentType: {paymentType}");
        }
        var templateValues = await _parameterService.GetAllParameterTemplateValuesAsync(MoneyTransferParameterGroup, parameter.ParameterCode);
        var templateDictionary = templateValues.ToDictionary(t => t.TemplateCode, t => t.TemplateValue);

        if (templateDictionary.TryGetValue("Permission", out var permissionValue) &&
            bool.TryParse(permissionValue, out var hasPermission) && hasPermission == false)
        {
            throw new PermissionDeniedExeption();
        }

        if (!templateDictionary.TryGetValue("DescriptionLength", out var lengthValue))
        {
            throw new NotFoundException($"DescriptionLength template for: {parameter.ParameterCode}");
        }

        if (!int.TryParse(lengthValue, out var minLength))
        {
            throw new NotFoundException($"Invalid DescriptionLength value for: {parameter.ParameterCode}");
        }

        if (description.Length < minLength)
        {
            throw new DescriptionLengthException(minLength);
        }
    }

    public async Task CheckDuplicateTransactionAsync(string idempotentKey)
    {
        if (!string.IsNullOrWhiteSpace(idempotentKey))
        {
            var transactionExists = await _transactionRepository
                .GetAll()
                .AnyAsync(s => s.IdempotentKey == idempotentKey);

            if (transactionExists)
            {
                throw new DuplicateRecordException();
            }
        }
    }
}