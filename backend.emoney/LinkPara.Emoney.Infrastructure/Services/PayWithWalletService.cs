using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Transactions;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;
using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.Transfer;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using AutoMapper;
using Azure.Identity;
using LinkPara.Emoney.Application.Features.PayWithWallets;
using LinkPara.HttpProviders.Identity.Models.Enums;
using Microsoft.Extensions.Configuration;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.TransferForLoggedInUser;

namespace LinkPara.Emoney.Infrastructure.Services;

public class PayWithWalletService : IPayWithWalletService
{
    private const string MobileChannel = "mobile";
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<WalletPaymentRequest> _walletPaymentRepository;
    private readonly ILogger<PayWithWalletService> _logger;
    private readonly IPricingProfileService _pricingProfileService;
    private readonly ILimitService _limitService;
    private readonly IUserService _userService;
    private readonly IAccountingService _accountingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IFraudTransactionService _transactionService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IBTransService _bTransService;
    private readonly IPushNotificationSender _pusNotificationSender;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IStringLocalizer _exceptionLocalizer;
    private readonly IVaultClient _vaultClient;
    private readonly ITierLevelService _tierLevelService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IStringLocalizer _errorLocalizer;
    private readonly IConfiguration _configuration;
    public PayWithWalletService(IGenericRepository<Wallet> walletRepository,
        IPricingCommercialService pricingCommercialService,
        ILogger<PayWithWalletService> logger,
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
        IMapper mapper,
        IGenericRepository<WalletPaymentRequest> walletPaymentRepository,
        IConfiguration configuration)
    {
        _walletRepository = walletRepository;
        _logger = logger;
        _pricingProfileService = pricingProfileService;
        _limitService = limitService;
        _userService = userService;
        _accountingService = accountingService;
        _auditLogService = auditLogService;
        _transactionService = transactionService;
        _contextProvider = contextProvider;
        _accountUserRepository = accountUserRepository;
        _parameterService = parameterService;
        _bTransService = bTransService;
        _pusNotificationSender = pusNotificationSender;
        _scopeFactory = scopeFactory;
        _tierLevelService = tierLevelService;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _exceptionLocalizer = stringLocalizerFactory.Create("Exceptions", "LinkPara.Emoney.API");
        _errorLocalizer = stringLocalizerFactory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _vaultClient = vaultClient;
        _saveReceiptService = saveReceiptService;
        _walletPaymentRepository = walletPaymentRepository;
        _configuration = configuration;
    }

    public async Task<PayWithWalletResponse> PayWithWalletAsync(PayWithWalletCommand request,
        CancellationToken cancellationToken)
    {
        var user = await GetUserByPhone(request.SenderPhoneNumber);
        if (user is null)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentUserNotFound")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = string.Format(exceptionMessage, request.SenderPhoneNumber) };
        }

        request.UserId = user.Id.ToString();

        var senderWallet = await GetMainWalletByPhoneAsync(Guid.Parse(request.UserId));

        if (senderWallet is null)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentSenderWalletNotFound")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
        }

        request.SenderWalletNumber = senderWallet.WalletNumber;

        var partnerWalletList = _vaultClient.GetSecretValue<List<PartnerWalletInfo>>("EmoneySecrets", "PartnerSettings", "PartnerWallets");
        var receiverPartner = partnerWalletList.FirstOrDefault(x => x.PartnerNumber == request.PartnerNumber);
        request.ReceiverWalletNumber = receiverPartner?.WalletNumber;

        var validatePaymentResult = await ValidatePaymentWithWallet(request);

        if (validatePaymentResult != null && validatePaymentResult.IsSuccess)
        {
            var receiverWallet = await _walletRepository.GetAll()
                .Include(t => t.Account)
                .SingleOrDefaultAsync(t => t.WalletNumber == request.ReceiverWalletNumber,
                    cancellationToken: cancellationToken);

            if (receiverWallet is null)
            {
                var exceptionMessage = _errorLocalizer.GetString("WalletPaymentReceiverWalletNotFound")?.Value;
                return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
            }

            ValidateWallets(senderWallet, receiverWallet);

            var pricing = await CalculatePricingAsync(senderWallet, request.Amount);

            var currencyCode = await GetCurrencyAsNumberAsync(senderWallet.CurrencyCode);

            var IsTransactionCheckEnabled = _vaultClient
                .GetSecretValue<bool>("/SharedSecrets", "ServiceState", "WalletPaymentFraudEnabled");

            if (IsTransactionCheckEnabled)
            {
                var requestModel = new FraudTransactionDetail
                {
                    Amount = request.Amount,
                    BeneficiaryNumber = request.ReceiverWalletNumber,
                    Beneficiary = receiverWallet.Account.Name,
                    OriginatorNumber = request.SenderWalletNumber,
                    Originator = senderWallet.Account.Name,
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
                    AccountKycLevel = senderWallet.Account.AccountKycLevel,
                    CommandJson = JsonConvert.SerializeObject(requestModel)
                });
            }

            request.IsLoggedIn = false;

            return await ExecuteTransferAsync(senderWallet, receiverWallet, request,
                pricing);
        }
        else
        {
            return validatePaymentResult;
        }
    }

    public async Task<PayWithWalletResponse> TransferForLoggedInUserAsync(TransferForLoggedInUserCommand request,
       CancellationToken cancellationToken)
    {
        var user = await GetUserByPhone(request.SenderPhoneNumber);
        if (user is null)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentUserNotFound")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = string.Format(exceptionMessage, request.SenderPhoneNumber) };
        }

        request.UserId = user.Id.ToString();

        var senderWallet = await GetMainWalletByPhoneAsync(Guid.Parse(request.UserId));

        if (senderWallet is null)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentSenderWalletNotFound")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
        }

        request.SenderWalletNumber = senderWallet.WalletNumber;

        var partnerWalletList = _vaultClient.GetSecretValue<List<PartnerWalletInfo>>("EmoneySecrets", "PartnerSettings", "PartnerWallets");
        var receiverPartner = partnerWalletList.FirstOrDefault(x => x.PartnerNumber == request.PartnerNumber);
        request.ReceiverWalletNumber = receiverPartner?.WalletNumber;

        var isValidateUser = request.UserId == _contextProvider.CurrentContext.UserId;

        if (isValidateUser)
        {
            var receiverWallet = await _walletRepository.GetAll()
                .Include(t => t.Account)
                .SingleOrDefaultAsync(t => t.WalletNumber == request.ReceiverWalletNumber,
                    cancellationToken: cancellationToken);

            if (receiverWallet is null)
            {
                var exceptionMessage = _errorLocalizer.GetString("WalletPaymentReceiverWalletNotFound")?.Value;
                return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
            }

            ValidateWallets(senderWallet, receiverWallet);

            var pricing = await CalculatePricingAsync(senderWallet, request.Amount);

            var currencyCode = await GetCurrencyAsNumberAsync(senderWallet.CurrencyCode);

            var IsTransactionCheckEnabled = _vaultClient
                .GetSecretValue<bool>("/SharedSecrets", "ServiceState", "WalletPaymentFraudEnabled");

            if (IsTransactionCheckEnabled)
            {
                var requestModel = new FraudTransactionDetail
                {
                    Amount = request.Amount,
                    BeneficiaryNumber = request.ReceiverWalletNumber,
                    Beneficiary = receiverWallet.Account.Name,
                    OriginatorNumber = request.SenderWalletNumber,
                    Originator = senderWallet.Account.Name,
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
                    AccountKycLevel = senderWallet.Account.AccountKycLevel,
                    CommandJson = JsonConvert.SerializeObject(requestModel)
                });
            }

            var tranferRequest = new PayWithWalletCommand() { 
                UserId = request.UserId,
                Amount = request.Amount,
                Description = request.Description,
                PartnerNumber = request.PartnerNumber,
                PaymentReferenceId = request.PaymentReferenceId,
                ReceiverWalletNumber = request.ReceiverWalletNumber,
                SenderPhoneNumber = request.SenderPhoneNumber,
                SenderWalletNumber = request.SenderWalletNumber,
                IsLoggedIn = true
        };

            return await ExecuteTransferAsync(senderWallet, receiverWallet, tranferRequest,
                pricing);
        }
        else
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentUserNotVerified")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
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

    private async Task<CalculatePricingResponse> CalculatePricingAsync(Wallet senderWallet, Decimal amount)
    {
        return await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
        {
            TransferType = TransferType.PaymentWithWallet,
            Amount = amount,
            BankCode = null,
            CurrencyCode = senderWallet.CurrencyCode,
            SenderWalletType = senderWallet.WalletType
        });

    }

    private async Task<string> GetCurrencyAsNumberAsync(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
         .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);

        return parameterTemplateValue?.FirstOrDefault(b => b.TemplateCode == "Number")?.TemplateValue;
    }

    private async Task<PayWithWalletLimit> GetLimitParametersAsync()
    {
        try
        {
            var tempVal = await _parameterService
         .GetAllParameterTemplateValuesAsync("PayWithWallet", "Limit");

            var result = new PayWithWalletLimit()
            {
                PaymentLimit = Convert.ToInt32(tempVal?.FirstOrDefault(b => b.TemplateCode == "PaymentLimit")?.TemplateValue),
                LoginExpireDayCount = Convert.ToInt32(tempVal?.FirstOrDefault(b => b.TemplateCode == "LoginExpireDayCount")?.TemplateValue),
                DailyTransactionCount = Convert.ToInt32(tempVal?.FirstOrDefault(b => b.TemplateCode == "DailyTransactionCount")?.TemplateValue)
            };
            return result;
        }
        catch (Exception e)
        {
            throw new NotFoundException($"PayWithWallet - Limit : {e.Message}");
        }

    }

    private async Task<PayWithWalletResponse> ExecuteTransferAsync(Wallet senderWallet, Wallet receiverWallet, PayWithWalletCommand request, CalculatePricingResponse pricing)
    {
        try
        {
            Account senderAccount = senderWallet.Account;
            Account receiverAccount = receiverWallet.Account;


            var senderTransactionId = Guid.Empty;
            var receiverTransactionId = Guid.Empty;
            decimal paymentAmount = 0;

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
                    var usableBalance = await GetUsableBalance(dbSenderWallet, pricing.TotalAmount, p2PCreditBalanceUsable);

                    if (usableBalance < pricing.TotalAmount)
                    {
                        if (pricing.PricingAmount > 0)
                        {
                            var tempPricing = await CalculatePricingAsync(senderWallet, usableBalance);
                            if (tempPricing != null)
                            {
                                usableBalance = usableBalance - (pricing.PricingAmount + pricing.BsmvTotal);
                                pricing = await CalculatePricingAsync(senderWallet, usableBalance);
                            }
                        }

                        request.Amount = usableBalance;
                    }


                    dbContext.Attach(dbSenderWallet);

                    var senderTransaction = PopulateSenderTransaction(dbSenderWallet, request, receiverAccount,
                        senderAccount, dbReceiverWallet.Id);

                    dbContext.Transaction.Add(senderTransaction);

                    Withdraw(dbSenderWallet, pricing.Amount, p2PCreditBalanceUsable);

                    if (pricing.PricingAmount > 0)
                    {
                        var senderPricingTransaction = PopulatePricingTransaction(dbSenderWallet, pricing, request.UserId,
                            PaymentMethod.Transfer, request.Description, senderTransaction.Id,
                            senderAccount, receiverAccount);

                        dbContext.Transaction.Add(senderPricingTransaction);

                        Withdraw(dbSenderWallet, pricing.PricingAmount, p2PCreditBalanceUsable);

                        var senderBsmvTransaction = PopulateBsmvTransaction(dbSenderWallet, pricing, request.UserId,
                            PaymentMethod.Transfer, request.Description, senderTransaction.Id, senderAccount,
                            receiverAccount);

                        dbContext.Transaction.Add(senderBsmvTransaction);

                        Withdraw(dbSenderWallet, pricing.BsmvTotal, p2PCreditBalanceUsable);
                    }

                    var receiverTransaction = PopulateReceiverTransaction(dbReceiverWallet, request, senderAccount, receiverAccount, senderWallet.Id);

                    dbContext.Transaction.Add(receiverTransaction);

                    dbContext.Attach(dbReceiverWallet);

                    Deposit(dbReceiverWallet, request.Amount);

                    var receiverAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                        senderAccount, receiverAccount, TransactionDirection.MoneyIn, request.UserId);
                    dbContext.AccountActivity.Add(receiverAccountActivity);

                    var senderAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                        senderAccount, receiverAccount, TransactionDirection.MoneyOut, request.UserId);
                    dbContext.AccountActivity.Add(senderAccountActivity);

                    var walletPaymentRequest = PopulateWalletPaymentRequest(dbSenderWallet, dbReceiverWallet, request, senderAccount, receiverAccount, senderTransaction.Id, senderTransaction.TransactionDate);
                    dbContext.WalletPaymentRequest.Add(walletPaymentRequest);

                    await dbContext.SaveChangesAsync();

                    senderTransactionId = senderTransaction.Id;
                    receiverTransactionId = receiverTransaction.Id;
                    paymentAmount = request.Amount;

                    await SendAccountingQueueAsync(senderWallet, receiverWallet, pricing, receiverTransaction, senderTransactionId);

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


                    await transactionScope.CommitAsync();
                }
                catch (Exception)
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                

            });

            await _saveReceiptService.SendReceiptQueueAsync(receiverTransactionId);
            await _saveReceiptService.SendReceiptQueueAsync(senderTransactionId);

            await SendNotifications(senderWallet, request.Amount);

            return new PayWithWalletResponse() { IsSuccess = true, TransactionId = senderTransactionId, PaymentAmount = paymentAmount };
        }
        catch (Exception exception)
        {
            if (exception is InsufficientBalanceException or LimitExceededException or CustomApiException)
            {
                throw;
            }

            _logger.LogError($"Payment With Wallet Error : {exception}");

            var details = new Dictionary<string, string>
                       {
                            {"SenderWalletNumber", senderWallet.WalletNumber.ToString() },
                            {"ReceiverWalletNumber", receiverWallet.WalletNumber.ToString() },
                            {"ErrorMessage" , exception.Message}
                       };
            await SendP2PMoneyTransferAuditLogAsync(false, Guid.Parse(request.UserId), details);

            return new PayWithWalletResponse
            {
                IsSuccess = false,
                ErrorMessage = exception.Message
            };
        }
    }

    private async Task<List<Wallet>> GetWalletsWithLockAsync(EmoneyDbContext dbContext, Guid senderWalletId, Guid receiverWalletId)
    {
        var databaseProvider = _configuration["DatabaseProvider"];
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

    private async Task<decimal> GetUsableBalance(Wallet wallet, decimal requestAmount, bool p2PCreditBalanceUsable = false)
    {
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        if (p2PCreditBalanceUsable)
        {
            if (requestAmount > wallet.AvailableBalance)
            {
                return wallet.AvailableBalance;
            }
        }
        else
        {
            if (requestAmount > wallet.AvailableBalanceCash)
            {
                return wallet.AvailableBalanceCash;
            }
        }

        return requestAmount;

    }

    private async Task<PayWithWalletResponse> ValidatePaymentWithWallet(PayWithWalletCommand request)
    {
        var response = new PayWithWalletResponse() {IsSuccess = true};

        var limitDto = await GetLimitParametersAsync();

        if (request.Amount > limitDto.PaymentLimit)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentAmountLimit")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = string.Format(exceptionMessage, limitDto.PaymentLimit) };
        }

        var userActivityResult = await _userService.GetUserLoginActivityByChannelAsync(Guid.Parse(request.UserId), MobileChannel);

        var lastMobileLoginActivity = userActivityResult?.LoginActivities.FirstOrDefault(x => x.LoginResult == LoginResult.Succeeded);

        var lastMobilAccessDate = lastMobileLoginActivity?.Date ?? DateTime.MinValue;

        if (lastMobilAccessDate == DateTime.MinValue || DateTime.Now > lastMobilAccessDate.AddDays(limitDto.LoginExpireDayCount))
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentLoginExpireDayCount")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = string.Format(exceptionMessage, limitDto.LoginExpireDayCount) };
        }
        
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var dailyTxnList = await _walletPaymentRepository.GetAll()
                                .Where(x => x.SenderWalletNo == request.SenderWalletNumber
                                 && x.PaymentReferenceId != request.PaymentReferenceId
                                 && x.TransactionDate >= today
                                 && x.TransactionDate < tomorrow
                                 && !x.IsLoggedIn)
                                .GroupBy(x => x.PaymentReferenceId)
                                .Select(g => g.First()).ToListAsync();

        if (dailyTxnList.Count >= limitDto.DailyTransactionCount)
        {
            var exceptionMessage = _errorLocalizer.GetString("WalletPaymentDailyTxnCount")?.Value;
            return new PayWithWalletResponse() { IsSuccess = false, ErrorMessage = string.Format(exceptionMessage, limitDto.DailyTransactionCount) };
        }

        return response;
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

    private Transaction PopulateSenderTransaction(Wallet senderWallet, PayWithWalletCommand request,
        Account receiverAccount, Account senderAccount, Guid receiverWalletId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.PayWithWallet,
            TransactionStatus = TransactionStatus.Completed,
            Tag = receiverAccount.Name,
            TagTitle = TransactionType.PayWithWallet.ToString(),
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
            Channel = _contextProvider.CurrentContext?.Channel
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
            Channel = _contextProvider.CurrentContext?.Channel
        };
    }

    private async Task IncreaseTransferLimitUsageAsync(EmoneyDbContext emoneyDbContext, Wallet wallet, LimitOperationType limitOpType, decimal amount, Guid userId)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == wallet.AccountId
                                       && x.CurrencyCode == wallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(wallet.CurrencyCode, wallet.AccountId, userId);
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = wallet.AccountId,
                LimitOperationType = limitOpType,
                Amount = amount,
                CurrencyCode = wallet.CurrencyCode,
                WalletType = wallet.WalletType
            }, level);
            emoneyDbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = wallet.AccountId,
                LimitOperationType = limitOpType,
                Amount = amount,
                CurrencyCode = wallet.CurrencyCode,
                WalletType = wallet.WalletType
            }, existingLevel);
            emoneyDbContext.Update(existingLevel);
        }
    }

    private Transaction PopulateReceiverTransaction(Wallet receiverWallet, PayWithWalletCommand request,
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
            Channel = _contextProvider.CurrentContext?.Channel
        };
    }

    private WalletPaymentRequest PopulateWalletPaymentRequest(Wallet senderWallet, Wallet receiverWallet, PayWithWalletCommand request, Account senderAccount, Account receiverAccount, Guid relatedTransactionId, DateTime transactionDate)
    {
        return new WalletPaymentRequest
        {
            PaymentReferenceId = request.PaymentReferenceId,
            InternalTransactionId = relatedTransactionId,
            Amount = request.Amount,
            CurrencyCode = senderWallet.CurrencyCode,
            SenderWalletNo = senderWallet.WalletNumber,
            ReceiverWalletNo = receiverWallet.WalletNumber,
            SenderName = senderAccount.Name,
            ReceiverName = receiverAccount.Name,
            CreateDate = DateTime.Now,
            CreatedBy = request.UserId,
            TransactionDate = transactionDate,
            IsLoggedIn = request.IsLoggedIn,
            Status = WalletPaymentStatus.Completed,
            RecordStatus = RecordStatus.Active
        };
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

    private void Withdraw(Wallet wallet, decimal amount, bool p2PCreditBalanceUsable = false)
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

        wallet.LastActivityDate = DateTime.Now;
    }

    private void Deposit(Wallet wallet, decimal amount)
    {
        wallet.CurrentBalanceCash += amount;
        wallet.LastActivityDate = DateTime.Now;
    }

    private async Task SendAccountingQueueAsync(Wallet senderWallet, Wallet receiverWallet, CalculatePricingResponse pricing, Transaction receiverTransaction, Guid senderTransactionId)
    {
        AccountingPayment payment = new AccountingPayment
        {
            Amount = receiverTransaction.Amount,
            BsmvAmount = pricing.BsmvTotal,
            CommissionAmount = pricing.PricingAmount,
            CurrencyCode = senderWallet.CurrencyCode,
            Destination = $"WA-{receiverWallet.WalletNumber}",
            HasCommission = pricing.PricingAmount > 0,
            OperationType = OperationType.PaymentWithWallet,
            Source = $"WA-{senderWallet.WalletNumber}",
            TransactionDate = receiverTransaction.TransactionDate,
            UserId = Guid.Empty,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            AccountingTransactionType = AccountingTransactionType.Emoney,
            TransactionId = senderTransactionId
        };

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

    private async Task SendP2PMoneyTransferAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = isSuccess,
                   LogDate = DateTime.Now,
                   Operation = "PayWithWallet",
                   SourceApplication = "Emoney",
                   Resource = "MoneyTransfer",
                   UserId = userId,
                   Details = details
               }
           );
    }

    private async Task<Wallet> GetMainWalletByPhoneAsync(Guid userId)
    {

        var accountUser = await _accountUserRepository.GetAll()
                        .FirstOrDefaultAsync(s => s.UserId == userId);

        if (accountUser is not null)
        {
            var mainWallet = await _walletRepository.GetAll(s => s.Currency)
                .Include(t => t.Account)
                .ThenInclude(t => t.AccountUsers)
                .SingleOrDefaultAsync(s =>
                    s.AccountId == accountUser.AccountId &&
                    s.RecordStatus == RecordStatus.Active &&
                    s.IsMainWallet);

            return mainWallet;
        }

        return new Wallet();

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

    private async Task SendNotifications(Wallet senderWallet, decimal amount)
    {
        try
        {
            Account senderAccount = senderWallet.Account;
            var sendPush = await GetNotificationParamAsync("Push");

            if (sendPush)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

                var senderTemplate = new Dictionary<string, string>
                {
                    { "walletName", senderWallet.FriendlyName },
                    { "amount", amount.ToString("N2") }
                };

                await PushSenderNotificationAsync(senderTemplate, senderAccount);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendNotificationsException : {exception}");
        }
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
            TemplateName = "PayWithWallet",
            TemplateParameters = templateData,
            Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = senderUserList
        };

        await _pusNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
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

    private async Task<UserDto> GetUserByPhone(string phoneNumber)
    {
        var user = await _userService.GetAllUsersAsync(new GetUsersRequest
        {
            PhoneNumber = phoneNumber,
            UserStatus = UserStatus.Active,
            UserType = UserType.Individual,
            RecordStatus = RecordStatus.Active
        });

        if (user is null || user.Items.Count == 0)
        {
            return null;
        }

        return user.Items.FirstOrDefault();
    }
}