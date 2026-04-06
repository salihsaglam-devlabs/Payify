using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.Provisions;
using LinkPara.Emoney.Application.Features.Provisions.Commands.CancelProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ReturnProvision;
using LinkPara.Emoney.Application.Features.Provisions.Queries.InquireProvision;
using LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview;
using LinkPara.Emoney.Application.Features.Wallets.Commands.CancelProvision;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Transactions;
using OnUsPaymentRequest = LinkPara.Emoney.Domain.Entities.OnUsPaymentRequest;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;
using TransactionStatus = LinkPara.Emoney.Domain.Enums.TransactionStatus;

namespace LinkPara.Emoney.Infrastructure.Services;

public class ProvisionService : IProvisionService
{
    private const string GeneralErrorCode = "500";

    private readonly EmoneyDbContext _dbContext;
    private readonly ILogger<ProvisionService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<Provision> _repository;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IGenericRepository<OnUsPaymentRequest> _onUsPaymentRequestRepository;
    private readonly ILimitService _limitService;
    private readonly ITierLevelService _tierLevelService;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _errorLocalizer;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IDatabaseProviderService _databaseProviderService;
    private readonly ICashbackService _cashbackService;

    public ProvisionService(EmoneyDbContext dbContext,
        ILogger<ProvisionService> logger,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<AccountUser> accountUserRepository,
        IApplicationUserService applicationUserService,
        IGenericRepository<Provision> repository,
        IUserService userService,
        IPushNotificationSender pushNotificationSender,
        IStringLocalizerFactory stringLocalizerFactory,
        IGenericRepository<OnUsPaymentRequest> onUsPaymentRequestRepository,
        ILimitService limitService,
        ITierLevelService tierLevelService,
        IGenericRepository<Wallet> walletRepository,
        IStringLocalizerFactory factory,
        IParameterService parameterService,
        ISaveReceiptService saveReceiptService,
        IDatabaseProviderService databaseProviderService,
        ICashbackService cashbackService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _accountUserRepository = accountUserRepository;
        _repository = repository;
        _userService = userService;
        _pushNotificationSender = pushNotificationSender;

        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _onUsPaymentRequestRepository = onUsPaymentRequestRepository;
        _limitService = limitService;
        _tierLevelService = tierLevelService;
        _walletRepository = walletRepository;
        _errorLocalizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _parameterService = parameterService;
        _saveReceiptService = saveReceiptService;
        _databaseProviderService = databaseProviderService;
        _cashbackService = cashbackService;
    }

    private async Task SendWalletProvisionNotificationAsync(AccountUser user, decimal amount, Wallet wallet,
    string formattedDateTime)
    {
        try
        {
            var senderUserList = new List<NotificationUserInfo>()

            {
                new NotificationUserInfo
                {
                    UserId = user.UserId,
                    FirstName = user.Firstname,
                    LastName = user.Lastname,
                }
            };

            var senderUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
            {
                UserIdList = senderUserList.Select(x => x.UserId).ToList(),
            });

            var senderNotificationRequest = new SendPushNotification
            {
                TemplateName = "WalletPayment",
                TemplateParameters = new Dictionary<string, string>
            {
                { "senderWalletName", wallet.FriendlyName },
                { "senderBalance", amount.ToString("N2")  },
                { "currentAvailableBalance", wallet.AvailableBalance.ToString("N2") },
                { "currentDate", formattedDateTime }
            },

                Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = senderUserList
            };

            await _pushNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnPushNotification \n{exception}");
        }
    }

    private async Task SendWalletProvisionCancelNotificationAsync(AccountUser user, decimal amount, string currencyCode, Wallet wallet,
        string formattedDateTime)
    {
        try
        {
            var senderUserList = new List<NotificationUserInfo>()
            {
                 new NotificationUserInfo
                {
                    UserId = user.UserId,
                    FirstName = user.Firstname,
                    LastName = user.Lastname,
                }
            };

            var senderUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
            {
                UserIdList = senderUserList.Select(x => x.UserId).ToList(),
            });

            var senderNotificationRequest = new SendPushNotification
            {
                TemplateName = "WalletPaymentCancel",
                TemplateParameters = new Dictionary<string, string>
            {
                { "senderWalletName", wallet.FriendlyName },
                { "currencyCode", currencyCode },
                { "senderBalance", amount.ToString("N2")  },
                { "currentDate", formattedDateTime }
            },

                Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = senderUserList
            };

            await _pushNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnPushNotification \n{exception}");
        }
    }

    private async Task SendWalletProvisionReturnNotificationAsync(AccountUser user, decimal amount, decimal returnAmount, string currencyCode, Wallet wallet,
        string formattedDateTime)
    {
        try
        {
            var senderUserList = new List<NotificationUserInfo>()
            {
                 new NotificationUserInfo
                {
                    UserId = user.UserId,
                    FirstName = user.Firstname,
                    LastName = user.Lastname,
                }
            };

            var senderUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
            {
                UserIdList = senderUserList.Select(x => x.UserId).ToList(),
            });

            var senderNotificationRequest = new SendPushNotification
            {
                TemplateName = (amount == returnAmount) ? "WalletPaymentReturn" : "WalletPaymentPartiallyReturn",
                TemplateParameters = new Dictionary<string, string>
            {
                { "senderWalletName", wallet.FriendlyName },
                { "amount", amount.ToString("N2")  },
                { "returnAmount", returnAmount.ToString("N2")  },
                { "currencyCode", currencyCode },
                { "currentDate", formattedDateTime }
            },

                Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = senderUserList
            };

            await _pushNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnPushNotification \n{exception}");
        }
    }

    public async Task<ProvisionResponse> ProvisionAsync(ProvisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            request.Amount = request.Amount.ToDecimal2();
            request.CommissionAmount = request.CommissionAmount.ToDecimal2();
            request.BsmvAmount = request.BsmvAmount.ToDecimal2();

            var provisionReference = string.Empty;
            var receiverName = string.Empty;
            var sourceWallet = new Wallet();
            var transactionId = Guid.Empty;
            OnUsPaymentRequest onUsPaymentRequest = null;
            if (request.ProvisionSource == ProvisionSource.Onus)
            {
                request.UserId = await GetUserIdFromWallet(request.WalletNumber);
            }

            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken: cancellationToken);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                sourceWallet = await GetWalletWithLockAsync(request.WalletNumber);
                _dbContext.Attach(sourceWallet);

                if (sourceWallet is null)
                {
                    throw new NotFoundException(nameof(Wallet), request.WalletNumber);
                }

                if (request.ProvisionSource == ProvisionSource.Partner)
                {
                    request.UserId = _applicationUserService.ApplicationUserId;
                }

                if (request.ProvisionSource == ProvisionSource.Onus)
                {
                    onUsPaymentRequest = await ValidateOnUsPayment(request, sourceWallet);
                    receiverName = onUsPaymentRequest.MerchantName;
                }

                var transaction = PopulateTransaction(request, sourceWallet, accountUser, receiverName);
                transactionId = transaction.Id;
                var provision = PopulateProvision(request, ProvisionStatus.Completed, transaction);

                if (request.ProvisionSource != ProvisionSource.Partner)
                {
                    await ValidateAccountAsync(request.UserId, sourceWallet);
                }

                ValidateWallets(sourceWallet, request.CurrencyCode);
                ValidateBalance(sourceWallet, request.Amount);

                Withdraw(sourceWallet, request.Amount);

                if (request.CommissionAmount > 0)
                {
                    var senderPricingTransaction = PopulatePricingCommissionTransaction(sourceWallet, request.CommissionAmount, request.UserId.ToString(),
                        PaymentMethod.Transfer, request.Description, transaction.Id,
                        sourceWallet.Account);

                    _dbContext.Transaction.Add(senderPricingTransaction);

                    Withdraw(sourceWallet, request.CommissionAmount);

                    var senderBsmvTransaction = PopulateBsmvTransaction(sourceWallet, request.BsmvAmount, request.UserId.ToString(),
                        PaymentMethod.Transfer, request.Description,
                        transaction.Id, sourceWallet.Account);

                    _dbContext.Transaction.Add(senderBsmvTransaction);

                    Withdraw(sourceWallet, request.BsmvAmount);
                }

                _dbContext.Add(transaction);
                _dbContext.Add(provision);

                if (request.ProvisionSource == ProvisionSource.Onus)
                {
                    onUsPaymentRequest.TransactionId = transaction.Id;
                    onUsPaymentRequest.TransactionDate = transaction.TransactionDate;
                    onUsPaymentRequest.WalletId = sourceWallet.Id;
                    onUsPaymentRequest.WalletNumber = sourceWallet.WalletNumber;
                    onUsPaymentRequest.ConversationId = provision.ConversationId;
                    onUsPaymentRequest.LastModifiedBy = request.UserId.ToString();
                    onUsPaymentRequest.UpdateDate = DateTime.Now;
                    await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentRequest);
                    await IncreaseLimitUsageAsync(sourceWallet, request.Amount, LimitOperationType.OnUs, _dbContext);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                transactionId = transaction.Id;
                provisionReference = provision.ProvisionReference;

                scope.Complete();
            });

            await AuditLogAsync(request, true);
            await _saveReceiptService.SendReceiptQueueAsync(transactionId);
            await SendWalletProvisionNotificationAsync(accountUser, request.Amount, sourceWallet, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            if (request.ProvisionSource == ProvisionSource.Billing)
            {
                var cashbackReq = new SendCashbackQueueRequest() { 
                    TransactionId = transactionId,
                    ConversationId = request.ConversationId
                };
                await _cashbackService.SendCashbackQueueAsync(cashbackReq);
            }

            return new ProvisionResponse
            {
                IsSucceed = true,
                ConversationId = request.ConversationId,
                ReferenceNumber = provisionReference,
                TransactionId = transactionId
            };

        }
        catch (Exception exception)
        {
            _logger.LogError($"Emoney Provision Error: {exception}");

            await AuditLogAsync(request, false);

            var provision = PopulateProvision(request, ProvisionStatus.Failed);
            provision.ErrorMessage = exception.Message;
            provision.ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode;

            _dbContext.Attach(provision);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorMessage = provision.ErrorMessage,
                ErrorCode = provision.ErrorCode,
                ConversationId = request.ConversationId,
                ReferenceNumber = provision.ProvisionReference
            };
        }
    }

    private async Task<OnUsPaymentRequest> ValidateOnUsPayment(ProvisionCommand request, Wallet wallet)
    {
        var onUsPaymentItem = await _onUsPaymentRequestRepository.GetAll()
                                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId
                                                        && x.Amount == request.Amount
                                                        && x.Status == Domain.Enums.OnUsPaymentStatus.Pending);

        if (onUsPaymentItem is null)
        {
            throw new NotFoundException(nameof(ProvisionCommand.OrderId), request.OrderId);
        }

        var validateLimit = await ValidateLimitsAsync(wallet, request);

        if (!validateLimit)
        {
            throw new LimitExceededException(LimitOperationType.OnUs);
        }

        return onUsPaymentItem;
    }

    public async Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceWallet = new Wallet();
            var transactionId = Guid.Empty;

            var provision = await _dbContext.Provision
                .FirstOrDefaultAsync(p => p.ConversationId == request.ConversationId);

            if (provision == null)
            {
                throw new NotFoundException(nameof(Provision), request.ConversationId);
            }

            var accountUser = await _accountUserRepository.GetAll()
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.UserId == provision.UserId);

            if (provision.ProvisionStatus is ProvisionStatus.Failed or ProvisionStatus.Returned)
            {
                throw new InvalidOperationException($"ProvisionCannotBeReturned: {provision.ConversationId}");
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                sourceWallet = await GetWalletWithLockAsync(provision.WalletNumber);
                _dbContext.Attach(sourceWallet);

                if (sourceWallet is null)
                {
                    throw new NotFoundException(nameof(Wallet), provision.WalletNumber);
                }

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var transaction = PopulateCancelProvisionTransaction(provision, sourceWallet);

                provision.ProvisionStatus = ProvisionStatus.Returned;

                _dbContext.Provision.Update(provision);

                if (provision.CommissionAmount > 0)
                {
                    var pricingTransactions = await _dbContext.Transaction
                        .Where(s => s.RelatedTransactionId == provision.TransactionId)
                        .ToListAsync();

                    foreach (var item in pricingTransactions)
                    {
                        var newTransaction = PopulateDepositTransaction(sourceWallet, item, accountUser.Account);
                        await _dbContext.AddAsync(newTransaction);
                        Deposit(sourceWallet, item.Amount);
                    }
                }

                _dbContext.Add(transaction);
                Deposit(sourceWallet, provision.Amount);

                if (provision.ProvisionSource == ProvisionSource.Onus)
                {
                    var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(sourceWallet.AccountId, sourceWallet.CurrencyCode);

                    await _limitService.DecreaseUsageAsync(new AccountLimitUpdateRequest
                    {
                        AccountId = sourceWallet.AccountId,
                        LimitOperationType = LimitOperationType.OnUs,
                        Amount = provision.Amount,
                        CurrencyCode = provision.CurrencyCode,
                        WalletType = WalletType.Individual
                    }, accountCurrentLevel);

                    var onUsPayment = _onUsPaymentRequestRepository.GetAll()
                                        .FirstOrDefault(x => x.ConversationId == request.ConversationId);

                    onUsPayment.Status = Domain.Enums.OnUsPaymentStatus.Canceled;

                    _dbContext.Update(onUsPayment);
                    _dbContext.Update(accountCurrentLevel);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                transactionId = transaction.Id;

                scope.Complete();
            });

            await _saveReceiptService.SendReceiptQueueAsync(transactionId);
            await SendWalletProvisionCancelNotificationAsync(accountUser, provision.Amount, provision.CurrencyCode, sourceWallet, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            return new ProvisionResponse
            {
                IsSucceed = true,
                ConversationId = request.ConversationId,
                TransactionId = transactionId
            };

        }
        catch (Exception exception)
        {
            _logger.LogError($"EMoney Cancel Provision Error: {exception}");

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = false,
                LogDate = DateTime.Now,
                Operation = "CancelProvision",
                SourceApplication = "Emoney",
                Resource = "UserCustomTier",
                Details = new Dictionary<string, string>
                {
                      {"ConversationId", request.ConversationId },
                      {"ErrorMessage", exception.Message }
                }
            });

            return new ProvisionResponse
            {
                IsSucceed = false,
                ConversationId = request.ConversationId,
                ErrorMessage = exception.Message,
                ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode
            };
        }
    }

    private async Task<Wallet> GetWalletWithLockAsync(string walletNumber)
    {
        var databaseProvider = await _databaseProviderService.GetProviderAsync();
        switch (databaseProvider)
        {
            case "MsSql":
                {
                    return await _dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM Core.Wallet WITH(ROWLOCK, UPDLOCK) " +
                                    "WHERE WalletNumber = {0} " +
                                    "AND RecordStatus = 'Active'", walletNumber)
                        .SingleOrDefaultAsync();
                }
            default:
                {
                    return await _dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM core.wallet " +
                                    "WHERE wallet_number = {0} " +
                                    "AND record_status = 'Active' FOR UPDATE", walletNumber)
                        .SingleOrDefaultAsync();
                }
        }
    }

    private Transaction PopulateDepositTransaction(Wallet wallet, Transaction item, Account receiverAccount)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Cancel,
            TransactionStatus = TransactionStatus.Completed,
            Tag = item.Tag,
            TagTitle = TransactionType.Cancel.ToString(),
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

    private async Task ValidateAccountAsync(Guid userId, Wallet sourceWallet)
    {
        var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (accountUser == null)
        {
            throw new NotFoundException(nameof(AccountUser), userId);
        }

        var account = accountUser.Account;
        if (account is null || account.AccountStatus is not AccountStatus.Active)
        {
            throw new NotFoundException(nameof(Account), userId);
        }

        if (sourceWallet.AccountId != account.Id)
        {
            throw new InvalidParameterException();
        }
    }

    private Provision PopulateProvision(ProvisionCommand request, ProvisionStatus status, Transaction transaction = null)
    {
        return new Provision
        {
            Amount = request.Amount,
            Description = $"{request.OrderId} {_tagLocalizer.GetString("Order")}",
            ConversationId = request.ConversationId,
            IsReturn = false,
            ProvisionSource = request.ProvisionSource,
            UserId = request.UserId,
            WalletNumber = request.WalletNumber,
            ClientIpAddress = request.ClientIpAddress ?? string.Empty,
            CurrencyCode = request.CurrencyCode,
            ProvisionStatus = status,
            TransactionId = transaction?.Id ?? Guid.Empty,
            CreatedBy = request.UserId.ToString(),
            PartnerId = request.PartnerId,
            ProvisionReference = GenerateProvisionReference(),
            CommissionAmount = request.CommissionAmount,
            BsmvAmount = request.BsmvAmount
        };
    }

    private static void ValidateWallets(Wallet sourceWallet, string currencyCode)
    {
        ValidateStatus(sourceWallet);

        if (sourceWallet.CurrencyCode != currencyCode)
        {
            throw new CurrencyCodeMismatchException();
        }
    }

    private static void ValidateStatus(Wallet wallet)
    {
        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException(wallet.WalletNumber);
        }
    }

    private static void ValidateBalance(Wallet wallet, decimal requestAmount)
    {        
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        if (requestAmount > wallet.AvailableBalance)
        {
            throw new InsufficientBalanceException();
        }
    }

    private Transaction PopulateCancelProvisionTransaction(Provision provision, Wallet sourceWallet)
    {
        string description = string.Empty;
        string senderName = string.Empty;
        string receiverName = string.Empty;
        if (provision.ProvisionSource == ProvisionSource.Onus)
        {
            var onUsPayment = _onUsPaymentRequestRepository.GetAll()
                                        .FirstOrDefault(x => x.TransactionId == provision.TransactionId);
            if (onUsPayment == null)
            {
                throw new NotFoundException(nameof(Provision), provision.TransactionId);
            }
            senderName = onUsPayment.MerchantName;
            receiverName = onUsPayment.UserName;
            description = $"{onUsPayment.OrderId} {_tagLocalizer.GetString("Order")} - {onUsPayment.MerchantName} - {_tagLocalizer.GetString(TransactionType.Cancel.ToString())}";
        }
        else
        {
            description = $"{provision.Description} - {_tagLocalizer.GetString(TransactionType.Cancel.ToString())}";
        }

        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Cancel,
            TransactionStatus = TransactionStatus.Completed,
            TagTitle = TransactionType.Cancel.ToString(),
            Tag = $"{_tagLocalizer.GetString(TransactionType.Cancel.ToString())}",
            Amount = provision.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = description,
            WalletId = sourceWallet.Id,
            CreatedBy = provision.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance + provision.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel,
            ReturnedTransactionId = provision.TransactionId,
            ReceiverName = receiverName,
            SenderName = senderName
        };
    }

    private Transaction PopulateTransaction(ProvisionCommand request, Wallet sourceWallet, AccountUser accountUser, string receiverName)
    {
        var transactionType = GetTransactionType(request.ProvisionSource);
        var description = $"{request.OrderId} {_tagLocalizer.GetString("Order")}";
        var tagTitle = transactionType.ToString();
        if (transactionType == TransactionType.OnUs)
        {
            description = $"{description} - {receiverName}";
            var tagParameter = _parameterService.GetParameterAsync("OnUsPayment", "OnUsTransactionTypeName");
            tagTitle = tagParameter.Result.ParameterValue;
        }

        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = transactionType,
            TransactionStatus = TransactionStatus.Completed,
            TagTitle = tagTitle,
            Tag = request.Tag,
            SenderName = $"{accountUser.Firstname} {accountUser.Lastname}",
            ReceiverName = receiverName,
            Amount = request.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = description,
            WalletId = sourceWallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance - request.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private static TransactionType GetTransactionType(ProvisionSource provisionSource)
    {
        return provisionSource switch
        {
            ProvisionSource.Billing => TransactionType.Billing,
            ProvisionSource.Epin => TransactionType.Epin,
            ProvisionSource.IWallet => TransactionType.IWallet,
            ProvisionSource.Onus => TransactionType.OnUs,
            _ => throw new ArgumentOutOfRangeException(nameof(provisionSource))
        };
    }

    private static void Withdraw(Wallet wallet, decimal amount)
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

        wallet.LastActivityDate = DateTime.Now;
    }

    private static void Deposit(Wallet wallet, decimal amount)
    {
        wallet.CurrentBalanceCredit += amount;
        wallet.LastActivityDate = DateTime.Now;
    }

    public async Task<ProvisionPreviewResponse> ProvisionPreviewAsync(ProvisionPreviewQuery request)
    {
        try
        {
            var wallet = _dbContext.Wallet.SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

            if (wallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            if (wallet.IsBlocked)
            {
                throw new WalletBlockedException(request.WalletNumber);
            }

            if (request.PartnerId == Guid.Empty)
            {
                await ValidateAccountAsync(request.UserId, wallet);
            }

            ValidateWallets(wallet, request.CurrencyCode);
            ValidateBalance(wallet, request.Amount);

            return new ProvisionPreviewResponse
            {
                IsSuccess = true,
            };
        }
        catch (Exception ex)
        {
            var response = new ProvisionPreviewResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };

            if (ex is ApiException apiException)
            {
                response.ErrorCode = apiException.Code;
            }

            return response;
        }
    }

    private async Task AuditLogAsync(ProvisionCommand request, bool isSuccess)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                LogDate = DateTime.Now,
                Operation = "Provision",
                SourceApplication = "Emoney",
                Resource = "Wallet",
                UserId = request.UserId,
                Details = new Dictionary<string, string>
                {
                    { "SourceWallet", request.WalletNumber },
                    { "ProvisionSource", request.ProvisionSource.ToString() },
                }
            }
        );
    }

    public async Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionQuery request)
    {
        Provision result = null;
        if (!string.IsNullOrEmpty(request.ConversationId)
            && !string.IsNullOrEmpty(request.ProvisionReference))
        {
            result = await _repository.GetAll()
                                      .Include(x => x.Transaction)
                                      .Where(x => x.ProvisionReference == request.ProvisionReference
                                               && x.ConversationId == request.ConversationId)
                                      .SingleOrDefaultAsync();
        }
        else if (!string.IsNullOrEmpty(request.ConversationId))
        {
            result = await _repository.GetAll()
                                      .Include(x => x.Transaction)
                                      .Where(x => x.ConversationId == request.ConversationId)
                                      .FirstOrDefaultAsync();
        }
        else if (!string.IsNullOrEmpty(request.ProvisionReference))
        {
            result = await _repository.GetAll()
                                      .Include(x => x.Transaction)
                                      .Where(x => x.ProvisionReference == request.ProvisionReference)
                                      .SingleOrDefaultAsync();
        }

        if (result is null)
        {
            return new InquireProvisionResponse
            {
                ErrorCode = ErrorCode.NotFound,
                ErrorDescription = _errorLocalizer["NotFound"]
            };
        }

        var returnedProvisionsAmount = GetAllReturnedProvisionAmount(result.Id);

        return new InquireProvisionResponse
        {
            ErrorCode = string.Empty,
            ErrorDescription = string.Empty,
            ConversationId = result.ConversationId,
            Description = result.Description,
            ProvisionStatus = result.ProvisionStatus,
            WalletNumber = result.WalletNumber,
            Amount = result.Amount,
            CurrencyCode = result.CurrencyCode,
            TransactionDate = result.Transaction.TransactionDate,
            TotalReturnedAmount = returnedProvisionsAmount
        };
    }

    private string GenerateProvisionReference()
    {
        bool any;
        string provisionReference;
        do
        {
            provisionReference = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().PadLeft(15, '0');
            any = _repository.GetAll().Any(s => s.ProvisionReference == provisionReference);
        }
        while (any);
        return provisionReference;
    }

    public async Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceWallet = _dbContext.Wallet
                .Include(x => x.Account)
                .SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s
                    => s.UserId == request.UserId, cancellationToken: cancellationToken);

            var sourceProvision = _dbContext.Provision
                .SingleOrDefault(s => s.ProvisionReference == request.ProvisionReference);

            if (sourceWallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            if (sourceProvision is null)
            {
                throw new NotFoundException(nameof(Provision), request.ProvisionReference);
            }

            if (request.Amount > sourceProvision.Amount)
            {
                throw new GreaterThanProvisionAmountException();
            }

            await ValidateAccountAsync(request.UserId, sourceWallet);
            ValidateWallets(sourceWallet, sourceProvision.CurrencyCode);

            var transaction = PopulateCashbackTransaction(request, sourceWallet, sourceProvision);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                _dbContext.Add(transaction);
                _dbContext.Attach(sourceWallet);
                Deposit(sourceWallet, request.Amount);

                await _dbContext.SaveChangesAsync(cancellationToken);

                scope.Complete();
            });

            await _saveReceiptService.SendReceiptQueueAsync(transaction.Id);

            return new ProvisionCashbackResponse
            {
                IsSucceed = true,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Emoney ProvisionCashback Error: {exception}");

            return new ProvisionCashbackResponse
            {
                IsSucceed = false,
                ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode,
                ErrorMessage = exception.Message
            };
        }
    }

    private Transaction PopulateCashbackTransaction(ProvisionCashbackCommand request, Wallet sourceWallet,
        Provision sourceProvision)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Cashback,
            TransactionStatus = TransactionStatus.Completed,
            Tag = $"{sourceProvision.Description} {_tagLocalizer.GetString(TransactionType.Cashback.ToString())}",
            TagTitle = TransactionType.Cashback.ToString(),
            Amount = request.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = _tagLocalizer.GetString(TransactionType.Cashback.ToString()),
            WalletId = sourceWallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance + request.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulatePricingCommissionTransaction(Wallet wallet, decimal amount,
        string userId, PaymentMethod paymentMethod, string description, Guid relatedTransactionId,
        Account senderAccount = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = TransactionStatus.Completed,
            Tag = _tagLocalizer.GetString(TransactionType.Commission.ToString()),
            TagTitle = TransactionType.Commission.ToString(),
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = description,
            WalletId = wallet.Id,
            CreatedBy = userId,
            CurrentBalance = wallet.AvailableBalance - amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            SenderName = senderAccount != null ? senderAccount.Name : null,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    private Transaction PopulateBsmvTransaction(Wallet wallet, decimal amount, string userId,
        PaymentMethod paymentMethod, string description, Guid relatedTransactionId,
        Account senderAccount = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = TransactionStatus.Completed,
            Tag = _tagLocalizer.GetString(TransactionType.Tax.ToString()),
            TagTitle = TransactionType.Tax.ToString(),
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = description,
            WalletId = wallet.Id,
            CreatedBy = userId,
            CurrentBalance = wallet.AvailableBalance - amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            RecordStatus = RecordStatus.Active,
            RelatedTransactionId = relatedTransactionId,
            SenderName = senderAccount?.Name,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty
        };
    }

    public async Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceWallet = _dbContext.Wallet
                .Include(x => x.Account)
                .SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

            var accountUser = await _accountUserRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken: cancellationToken);

            var provision = _dbContext.Provision
                .SingleOrDefault(s => s.ProvisionReference == request.ProvisionReference);

            if (sourceWallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            if (provision is null)
            {
                throw new NotFoundException(nameof(Provision), request.ProvisionReference);
            }

            if (request.Amount > provision.Amount)
            {
                throw new GreaterThanProvisionAmountException();
            }

            await ValidateAccountAsync(request.UserId, sourceWallet);
            ValidateWallets(sourceWallet, provision.CurrencyCode);

            var transaction = PopulateCashbackReturnTransaction(request, sourceWallet, provision);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                _dbContext.Add(transaction);
                _dbContext.Attach(sourceWallet);
                Withdraw(sourceWallet, request.Amount);

                await _dbContext.SaveChangesAsync(cancellationToken);

                scope.Complete();
            });

            await _saveReceiptService.SendReceiptQueueAsync(transaction.Id);

            return new ProvisionCashbackResponse
            {
                IsSucceed = true,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Emoney ProvisionCashbackReturn Error: {exception}");

            return new ProvisionCashbackResponse
            {
                IsSucceed = false,
                ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode,
                ErrorMessage = exception.Message
            };
        }
    }

    private Transaction PopulateCashbackReturnTransaction(CancelProvisionCashbackCommand request, Wallet sourceWallet,
        Provision provision)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Return,
            TransactionStatus = TransactionStatus.Completed,
            Tag = $"{provision.Description} " +
                  $"{_tagLocalizer.GetString(TransactionType.Cashback.ToString())} " +
                  $"{_tagLocalizer.GetString(TransactionType.Return.ToString())}",
            TagTitle = TransactionType.Return.ToString(),
            Amount = request.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = "CashbackReturn",
            WalletId = sourceWallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance - request.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel
        };
    }

    public async Task<ProvisionResponse> ReturnProvisionAsync(ReturnProvisionCommand request, CancellationToken cancellationToken)
    {

        try
        {
            var sourceWallet = _dbContext.Wallet
                .Include(x => x.Account)
                .SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

            if (request.ProvisionSource == ProvisionSource.Onus)
            {
                request.UserId = await GetUserIdFromWallet(request.WalletNumber);
            }

            var accountUser = await _accountUserRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            var referenceProvision = _dbContext.Provision
                .SingleOrDefault(s => s.ProvisionReference == request.ProvisionReferenceNumber);

            if (sourceWallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            if (referenceProvision is null)
            {
                throw new NotFoundException(nameof(Provision), request.ProvisionReferenceNumber);
            }

            decimal totalReturnedAmount = GetAllReturnedTotalAmount(request, referenceProvision);

            if (totalReturnedAmount > referenceProvision.Amount)
            {
                throw new GreaterThanProvisionAmountException();
            }

            if (request.Amount == referenceProvision.Amount)
            {
                if (request.ProvisionSource != ProvisionSource.Onus)
                {
                    return await CancelProvisionAsync(new CancelProvisionCommand { ConversationId = referenceProvision.ConversationId }, cancellationToken);
                }
            }

            ProvisionStatus referenceProvisionStatus = CalculateReferenceProvisionStatus(totalReturnedAmount, referenceProvision);

            await ValidateAccountAsync(request.UserId, sourceWallet);
            ValidateWallets(sourceWallet, referenceProvision.CurrencyCode);


            var transaction = PopulatePatiallyReturnTransaction(request, sourceWallet, referenceProvision);
            var returnedProvision = PopulateReturnProvision(request, ProvisionStatus.Returned, referenceProvision.Id, transaction);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (returnedProvision.ProvisionSource == ProvisionSource.Onus)
                {
                    var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(sourceWallet.AccountId, sourceWallet.CurrencyCode);

                    await _limitService.DecreaseUsageAsync(new AccountLimitUpdateRequest
                    {
                        AccountId = sourceWallet.AccountId,
                        LimitOperationType = LimitOperationType.OnUs,
                        Amount = returnedProvision.Amount,
                        CurrencyCode = returnedProvision.CurrencyCode,
                        WalletType = WalletType.Individual
                    }, accountCurrentLevel);

                    var onUsPayment = _onUsPaymentRequestRepository.GetAll()
                                        .FirstOrDefault(x => x.ConversationId == request.ConversationId);
                    if (referenceProvisionStatus == ProvisionStatus.PartiallyReturned)
                    {
                        onUsPayment.Status = Domain.Enums.OnUsPaymentStatus.PartiallyReturned;
                    }
                    else if (referenceProvisionStatus == ProvisionStatus.Returned)
                    {
                        onUsPayment.Status = Domain.Enums.OnUsPaymentStatus.Returned;
                    }

                    _dbContext.Update(onUsPayment);
                    _dbContext.Update(accountCurrentLevel);
                }

                _dbContext.Add(transaction);
                _dbContext.Add(returnedProvision);
                _dbContext.Attach(sourceWallet);
                _dbContext.Attach(referenceProvision);
                UpdateReferenceProvision(referenceProvision, referenceProvisionStatus);

                Deposit(sourceWallet, request.Amount);

                await _dbContext.SaveChangesAsync(cancellationToken);

                scope.Complete();
            });

            await _saveReceiptService.SendReceiptQueueAsync(transaction.Id);

            await SendWalletProvisionReturnNotificationAsync(accountUser, referenceProvision.Amount, request.Amount, request.CurrencyCode, sourceWallet, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            return new ProvisionResponse
            {
                IsSucceed = true,
                ConversationId = request.ConversationId,
                ReferenceNumber = returnedProvision.ProvisionReference,
                TransactionId = transaction.Id
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Emoney ProvisionReturn Error: {exception}");

            var referenceProvision = _dbContext.Provision.SingleOrDefault(s => s.ProvisionReference == request.ProvisionReferenceNumber);

            NullControlHelper.CheckAndThrowIfNull(referenceProvision, request.ProvisionReferenceNumber, _logger);

            var provision = PopulateReturnProvision(request, ProvisionStatus.Failed, referenceProvision.Id);

            provision.ErrorMessage = exception.Message;
            provision.ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode;

            _dbContext.Attach(provision);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorMessage = provision.ErrorMessage,
                ErrorCode = provision.ErrorCode,
                ConversationId = request.ConversationId,
                ReferenceNumber = provision.ProvisionReference
            };
        }
    }

    private static void UpdateReferenceProvision(Provision referenceProvision, ProvisionStatus status)
    {
        referenceProvision.ProvisionStatus = status;
        referenceProvision.ReturnDate = DateTime.Now;
        referenceProvision.IsReturn = true;
    }

    private decimal GetAllReturnedTotalAmount(ReturnProvisionCommand request, Provision referenceProvision)
    {
        var returnedProvisions = _dbContext.Provision.AsQueryable().Where(s => s.PaymentProvisionId == referenceProvision.Id).ToList();

        decimal totalReturnedAmount = 0;
        if (returnedProvisions is not null)
        {
            totalReturnedAmount = returnedProvisions.Sum(s => s.Amount);
        }

        totalReturnedAmount += request.Amount;
        return totalReturnedAmount;
    }

    private ProvisionStatus CalculateReferenceProvisionStatus(decimal totalAmount, Provision referenceProvision)
    {
        return totalAmount == referenceProvision.Amount ? ProvisionStatus.Returned : ProvisionStatus.PartiallyReturned;
    }

    private Transaction PopulatePatiallyReturnTransaction(ReturnProvisionCommand request, Wallet sourceWallet, Provision referenceProvision)
    {
        string description = string.Empty;
        string senderName = string.Empty;
        if (request.ProvisionSource == ProvisionSource.Onus)
        {
            var onUsPayment = _onUsPaymentRequestRepository.GetAll()
                                        .FirstOrDefault(x => x.ConversationId == request.ConversationId);
            if (onUsPayment == null)
            {
                throw new NotFoundException(nameof(Provision), request.ConversationId);
            }
            senderName = onUsPayment.MerchantName;
            description = $"{onUsPayment.OrderId} " +
                          $"{_tagLocalizer.GetString("Order")} - {onUsPayment.MerchantName} - " +
                          $"{_tagLocalizer.GetString(TransactionType.Return.ToString())}";
        }
        else
        {
            description = $"{request.Description} - {_tagLocalizer.GetString(TransactionType.Return.ToString())}";
        }

        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Return,
            TransactionStatus = TransactionStatus.Completed,
            Tag = $"{referenceProvision.Description} " +
                  $"{_tagLocalizer.GetString(TransactionType.IWallet.ToString())} " +
                  $"{_tagLocalizer.GetString(TransactionType.Return.ToString())}",
            TagTitle = TransactionType.Return.ToString(),
            Amount = request.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = description,
            WalletId = sourceWallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance + request.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel,
            ReturnedTransactionId = referenceProvision.TransactionId,
            ReceiverName = sourceWallet.Account.Name,
            SenderName = senderName
        };
    }

    private Provision PopulateReturnProvision(ReturnProvisionCommand request, ProvisionStatus status, Guid? paymentProvisionId = null, Transaction transaction = null)
    {
        return new Provision
        {
            Amount = request.Amount,
            Description = request.Description,
            ConversationId = request.ConversationId,
            IsReturn = true,
            ProvisionSource = request.ProvisionSource,
            UserId = request.UserId,
            WalletNumber = request.WalletNumber,
            ClientIpAddress = request.ClientIpAddress,
            CurrencyCode = request.CurrencyCode,
            ProvisionStatus = status,
            TransactionId = transaction?.Id ?? Guid.Empty,
            CreatedBy = request.UserId.ToString(),
            ProvisionReference = GenerateProvisionReference(),
            PaymentProvisionId = paymentProvisionId
        };
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

    private async Task<bool> ValidateLimitsAsync(Wallet wallet, ProvisionCommand command)
    {
        var response = await _limitService.IsLimitExceededAsync(new LimitControlRequest
        {
            Amount = command.Amount,
            CurrencyCode = command.CurrencyCode,
            LimitOperationType = LimitOperationType.OnUs,
            AccountId = wallet.AccountId,
            WalletNumber = wallet.WalletNumber
        });

        return !response.IsLimitExceeded;
    }

    private async Task<Guid> GetUserIdFromWallet(string walletNumber)
    {
        var wallet = await _walletRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.WalletNumber == walletNumber);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Wallet), walletNumber);
        }

        var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.AccountId == wallet.AccountId);

        if (accountUser == null)
        {
            throw new NotFoundException(nameof(Account), wallet.AccountId);
        }

        return accountUser.UserId;
    }

    public async Task<ProvisionChargebackResponse> ProvisionChargebackAsync(ProvisionChargebackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceWallet = _dbContext.Wallet
                .Include(x => x.Account)
                .SingleOrDefault(s => s.WalletNumber == request.WalletNumber);

            if (request.ProvisionSource == ProvisionSource.Onus)
            {
                request.UserId = await GetUserIdFromWallet(request.WalletNumber);
            }

            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s
                    => s.UserId == request.UserId, cancellationToken: cancellationToken);

            var sourceProvision = _dbContext.Provision
                .SingleOrDefault(s => s.ProvisionReference == request.ProvisionReference);

            if (sourceWallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            if (sourceProvision is null)
            {
                throw new NotFoundException(nameof(Provision), request.ProvisionReference);
            }

            var returnedProvisions = GetAllReturnedProvisionAmount(sourceProvision.Id);
            if (returnedProvisions + request.Amount > sourceProvision.Amount)
            {
                throw new GreaterThanProvisionAmountException();
            }

            ProvisionStatus referenceProvisionStatus = (returnedProvisions + request.Amount) == sourceProvision.Amount ? ProvisionStatus.Chargeback : ProvisionStatus.PartiallyReturned;
            UpdateReferenceProvision(sourceProvision, referenceProvisionStatus);

            await ValidateAccountAsync(request.UserId, sourceWallet);
            ValidateWallets(sourceWallet, sourceProvision.CurrencyCode);

            var transaction = PopulateChargebackTransaction(request, sourceWallet, sourceProvision);
            var returnedProvision = PopulateChargebackProvision(request, ProvisionStatus.Chargeback, sourceProvision.Id, transaction);
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (sourceProvision.ProvisionSource == ProvisionSource.Onus)
                {
                    var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(sourceWallet.AccountId, sourceWallet.CurrencyCode);

                    await _limitService.DecreaseUsageAsync(new AccountLimitUpdateRequest
                    {
                        AccountId = sourceWallet.AccountId,
                        LimitOperationType = LimitOperationType.OnUs,
                        Amount = sourceProvision.Amount,
                        CurrencyCode = sourceProvision.CurrencyCode,
                        WalletType = WalletType.Individual
                    }, accountCurrentLevel);

                    _dbContext.Update(accountCurrentLevel);
                }

                _dbContext.Add(transaction);
                _dbContext.Add(returnedProvision);
                _dbContext.Attach(sourceWallet);
                Deposit(sourceWallet, request.Amount);
                _dbContext.Update(sourceProvision);
                await _dbContext.SaveChangesAsync(cancellationToken);

                scope.Complete();
            });

            await _saveReceiptService.SendReceiptQueueAsync(transaction.Id);

            return new ProvisionChargebackResponse
            {
                IsSucceed = true,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Emoney ProvisionCashback Error: {exception}");

            return new ProvisionChargebackResponse
            {
                IsSucceed = false,
                ErrorCode = exception is ApiException apiException
                ? apiException.Code
                : GeneralErrorCode,
                ErrorMessage = exception.Message
            };
        }
    }


    private Transaction PopulateChargebackTransaction(ProvisionChargebackCommand request, Wallet sourceWallet,
        Provision sourceProvision)
    {
        string description = string.Empty;
        string senderName = string.Empty;
        if (request.ProvisionSource == ProvisionSource.Onus)
        {
            var onUsPayment = _onUsPaymentRequestRepository.GetAll()
                                        .FirstOrDefault(x => x.ConversationId == request.ConversationId);
            if (onUsPayment == null)
            {
                throw new NotFoundException(nameof(Provision), request.ConversationId);
            }
            senderName = onUsPayment.MerchantName;
            description = $"{onUsPayment.OrderId} " +
                          $"{_tagLocalizer.GetString("Order")} - {onUsPayment.MerchantName} - " +
                          $"{_tagLocalizer.GetString(TransactionType.Chargeback.ToString())}";
        }
        else
        {
            description = $"{request.Description} - {_tagLocalizer.GetString(TransactionType.Chargeback.ToString())}";
        }

        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Chargeback,
            TransactionStatus = TransactionStatus.Completed,
            Tag = $"{sourceProvision.Description} {_tagLocalizer.GetString(TransactionType.Chargeback.ToString())}",
            TagTitle = TransactionType.Chargeback.ToString(),
            Amount = request.Amount,
            CurrencyCode = sourceWallet.CurrencyCode,
            Description = description,
            WalletId = sourceWallet.Id,
            CreatedBy = request.UserId.ToString(),
            CurrentBalance = sourceWallet.AvailableBalance + request.Amount,
            PreBalance = sourceWallet.AvailableBalance,
            PaymentMethod = PaymentMethod.Provision,
            TransactionDate = DateTime.Now,
            Channel = _contextProvider.CurrentContext?.Channel,
            ReturnedTransactionId = sourceProvision.TransactionId,
            ReceiverName = sourceWallet.Account.Name,
            SenderName = senderName
        };
    }

    private Provision PopulateChargebackProvision(ProvisionChargebackCommand request, ProvisionStatus status, Guid? paymentProvisionId = null, Transaction transaction = null)
    {
        return new Provision
        {
            Amount = request.Amount,
            Description = request.Description,
            ConversationId = request.ConversationId,
            IsReturn = true,
            ProvisionSource = request.ProvisionSource,
            UserId = request.UserId,
            WalletNumber = request.WalletNumber,
            ClientIpAddress = request.ClientIpAddress,
            CurrencyCode = request.CurrencyCode,
            ProvisionStatus = status,
            TransactionId = transaction?.Id ?? Guid.Empty,
            CreatedBy = request.UserId.ToString(),
            ProvisionReference = GenerateProvisionReference(),
            PaymentProvisionId = paymentProvisionId
        };
    }

    private decimal GetAllReturnedProvisionAmount(Guid sourceProvisionId)
    {
        var returnedProvisions = _dbContext.Provision.AsQueryable().Where(s => s.PaymentProvisionId == sourceProvisionId).ToList();

        decimal totalReturnedAmount = 0;
        if (returnedProvisions is not null)
        {
            totalReturnedAmount = returnedProvisions.Sum(s => s.Amount);
        }

        return totalReturnedAmount;
    }


}