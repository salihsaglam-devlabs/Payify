using LinkPara.HttpProviders.Cashback.Models;
using LinkPara.HttpProviders.Cashback.Enums;
using MassTransit;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Logging;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.HttpProviders.Vault;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LinkPara.Emoney.Application.Features.Cashback;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Domain.Enums;
using Microsoft.Extensions.Configuration;
using LinkPara.ContextProvider;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.Audit;
using LinkPara.Emoney.Application.Commons.Interfaces;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.HttpProviders.Identity;
using LinkPara.SystemUser;
using LinkPara.HttpProviders.DbProvider;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class CashbackTransferConsumer : IConsumer<CashbackTransferRequest>
{
    private readonly IBus _bus;
    private readonly IGenericRepository<CashbackPaymentRequest> _paymentRequestRepository;
    private readonly ILogger<CashbackTransferConsumer> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IStringLocalizer _errorLocalizer;
    private readonly IParameterService _parameterService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IContextProvider _contextProvider;
    private readonly IAccountingService _accountingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IBTransService _bTransService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IUserService _userService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IDatabaseProviderService _databaseProviderService;



    public CashbackTransferConsumer(IBus bus,
        IGenericRepository<CashbackPaymentRequest> paymentRequestRepository,
        ILogger<CashbackTransferConsumer> logger,
        IVaultClient vaultClient,
        IGenericRepository<Wallet> walletRepository,
        IStringLocalizerFactory stringLocalizerFactory,
        IParameterService parameterService,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IContextProvider contextProvider,
        IAccountingService accountingService,
        IAuditLogService auditLogService,
        IBTransService bTransService,
        ISaveReceiptService saveReceiptService,
        IUserService userService,
        IApplicationUserService applicationUserService,
        IDatabaseProviderService databaseProviderService)
    {
        _bus = bus;
        _paymentRequestRepository = paymentRequestRepository;
        _logger = logger;
        _vaultClient = vaultClient;
        _walletRepository = walletRepository;
        _errorLocalizer = stringLocalizerFactory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _parameterService = parameterService;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _contextProvider = contextProvider;
        _accountingService = accountingService;
        _auditLogService = auditLogService;
        _bTransService = bTransService;
        _saveReceiptService = saveReceiptService;
        _userService = userService;
        _applicationUserService = applicationUserService;
        _databaseProviderService = databaseProviderService;
    }
    public async Task Consume(ConsumeContext<CashbackTransferRequest> context)
    {
        var request = context.Message;
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        var consumerRequest = new CashbackTransferCompletedRequest()
        {
            EntitlementId = request.EntitlementId,
            PaymentDate = DateTime.Now
        };

        var paymentRequest = new CashbackPaymentRequest()
        {
            EntitlementId = request.EntitlementId,
            CashbackPaymentStatus = CashbackPaymentStatus.Unpaid,
            WalletNumber = request.WalletNumber,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        };

        try
        {
            var result = await CashbackTransferAsync(request, cancellationTokenSource.Token);

            if (result.IsSuccess)
            {
                consumerRequest.CashbackPaymentStatus = CashbackPaymentStatus.Paid;
                paymentRequest.CashbackPaymentStatus = CashbackPaymentStatus.Paid;
                paymentRequest.TransactionId = result.TransactionId;
                var receiverAccount = result.ReceiverAccount;
                if (receiverAccount != null && receiverAccount.Id != Guid.Empty)
                {
                    var notificationInfo = await GetPushNotificationInfoAsync(receiverAccount);
                    consumerRequest.NotificationInfo = notificationInfo;
                }
            }
            else
            {
                consumerRequest.CashbackPaymentStatus = CashbackPaymentStatus.Unpaid;
                consumerRequest.FailedReason = result.ErrorMessage;
                _logger.LogError($"Cashback Transfer Error : {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            consumerRequest.CashbackPaymentStatus = CashbackPaymentStatus.Unpaid;
            consumerRequest.FailedReason = ex.Message;
            _logger.LogError(ex, "Cashback transfer process failed.");

        }
        finally
        {

            await _paymentRequestRepository.AddAsync(paymentRequest);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Cashback.CashbackTransferCompletedRequest"));
            await endpoint.Send(consumerRequest, tokenSource.Token);
        }

    }

    private async Task<CashbackNotificationDto> GetPushNotificationInfoAsync(Account account)
    {
        var senderUserList = account.AccountUsers
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



        var response = new CashbackNotificationDto()
        {
            RegistrationTokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = senderUserList
        };

        return response;
    }

    private async Task<CashbackTransferResponse> CashbackTransferAsync(CashbackTransferRequest request, CancellationToken cancellationToken)
    {
        var receiverWalletNumber = request.WalletNumber;

        var senderWalletInfo = _vaultClient.GetSecretValue<CashbackWallet>("EmoneySecrets", "CashbackSettings");
        var senderWalletNumber = senderWalletInfo.SenderWalletNumber;

        if (String.IsNullOrEmpty(senderWalletNumber))
        {
            var exceptionMessage = _errorLocalizer.GetString("CashbackSenderWalletNumberNonTaken")?.Value;
            return new CashbackTransferResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
        }

        var receiverWallet = await _walletRepository.GetAll()
               .Include(t => t.Account)
               .ThenInclude(t => t.AccountUsers)
               .SingleOrDefaultAsync(t => t.WalletNumber == receiverWalletNumber,
                   cancellationToken: cancellationToken);

        var senderWallet = await _walletRepository.GetAll()
               .Include(t => t.Account)
               .SingleOrDefaultAsync(t => t.WalletNumber == senderWalletNumber,
                   cancellationToken: cancellationToken);

        if (receiverWallet is null || senderWallet is null)
        {
            var exceptionMessage = receiverWallet == null ? _errorLocalizer.GetString("CashbackReceiverWalletNotFound")?.Value : _errorLocalizer.GetString("CashbackSenderWalletNotFound")?.Value;
            return new CashbackTransferResponse() { IsSuccess = false, ErrorMessage = exceptionMessage };
        }

        ValidateWallets(senderWallet, receiverWallet);

        return await ExecuteTransferAsync(senderWallet, receiverWallet, request);
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
        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException(wallet.WalletNumber);
        }
    }

    private async Task<CashbackTransferResponse> ExecuteTransferAsync(Wallet senderWallet, Wallet receiverWallet, CashbackTransferRequest request)
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

                    dbContext.Attach(dbSenderWallet);

                    var senderTransaction = PopulateSenderTransaction(dbSenderWallet, request, receiverAccount,
                        senderAccount, dbReceiverWallet.Id);

                    dbContext.Transaction.Add(senderTransaction);

                    if (dbSenderWallet.CurrentBalanceCash < request.Amount)
                    {
                        throw new Exception("Gönderici bakiyesi yetersiz.");
                    }
                    Withdraw(dbSenderWallet, request.Amount);

                    var receiverTransaction = PopulateReceiverTransaction(dbReceiverWallet, request, senderAccount, receiverAccount, senderWallet.Id);

                    dbContext.Transaction.Add(receiverTransaction);

                    dbContext.Attach(dbReceiverWallet);

                    Deposit(dbReceiverWallet, request.Amount);

                    var receiverAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                            senderAccount, receiverAccount, TransactionDirection.MoneyIn);
                    dbContext.AccountActivity.Add(receiverAccountActivity);

                    var senderAccountActivity = PopulateAccountTransfer(dbSenderWallet, dbReceiverWallet, request.Amount,
                        senderAccount, receiverAccount, TransactionDirection.MoneyOut);
                    dbContext.AccountActivity.Add(senderAccountActivity);

                    await dbContext.SaveChangesAsync();

                    senderTransactionId = senderTransaction.Id;
                    receiverTransactionId = receiverTransaction.Id;
                    paymentAmount = request.Amount;

                    await transactionScope.CommitAsync();

                    await SendAccountingQueueAsync(senderWallet, receiverWallet, receiverTransaction, senderTransactionId);

                    var details = new Dictionary<string, string>
                                      {
                                      {"SenderWalletNumber", dbSenderWallet.WalletNumber.ToString() },
                                      {"ReceiverWalletNumber", dbReceiverWallet.WalletNumber.ToString() },
                                      {"TransactionId" ,receiverTransaction.Id.ToString() }
                                      };

                    await SendP2PMoneyTransferAuditLogAsync(true, details);

                    _ = Task.Run(() =>
                        SendTransferBTransQueueAsync(dbSenderWallet, senderAccount, dbReceiverWallet,
                            receiverAccount, senderTransaction));
                }
                catch (Exception ex)
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }
            });

            await _saveReceiptService.SendReceiptQueueAsync(receiverTransactionId);
            await _saveReceiptService.SendReceiptQueueAsync(senderTransactionId);

            return new CashbackTransferResponse() { IsSuccess = true, ReceiverAccount = receiverAccount, TransactionId = senderTransactionId, PaymentAmount = paymentAmount };
        }
        catch (Exception ex)
        {
            var details = new Dictionary<string, string>
                       {
                            {"SenderWalletNumber", senderWallet.WalletNumber.ToString() },
                            {"ReceiverWalletNumber", receiverWallet.WalletNumber.ToString() },
                            {"ErrorMessage" , ex.Message}
                       };
            await SendP2PMoneyTransferAuditLogAsync(false, details);

            return new CashbackTransferResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
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

    private Transaction PopulateSenderTransaction(Wallet senderWallet, CashbackTransferRequest request,
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
            Description = request.RuleDescription,
            WalletId = senderWallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
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

    private Transaction PopulateReceiverTransaction(Wallet receiverWallet, CashbackTransferRequest request,
        Account senderAccount, Account receiverAccount, Guid senderWalletId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.Cashback,
            TransactionStatus = TransactionStatus.Completed,
            Tag = senderAccount.Name,
            TagTitle = TransactionType.Cashback.ToString(),
            Amount = request.Amount,
            CurrencyCode = receiverWallet.CurrencyCode,
            Description = request.RuleDescription,
            WalletId = receiverWallet.Id,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
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
    private void Withdraw(Wallet wallet, decimal amount, bool p2PCreditBalanceUsable = false)
    {
        if (p2PCreditBalanceUsable)
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

    private AccountActivity PopulateAccountTransfer(Wallet senderWallet, Wallet receiverWallet,
        decimal amount, Account senderAccount, Account receiverAccount, TransactionDirection transactionDirection)
    {
        return new AccountActivity
        {
            Id = Guid.NewGuid(),
            AccountId = transactionDirection == TransactionDirection.MoneyIn ? receiverAccount.Id : senderAccount.Id,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            LastModifiedBy = "",
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

    private async Task SendAccountingQueueAsync(Wallet senderWallet, Wallet receiverWallet, Transaction receiverTransaction, Guid senderTransactionId)
    {
        AccountingPayment payment = new AccountingPayment
        {
            Amount = receiverTransaction.Amount,
            BsmvAmount = 0,
            CommissionAmount = 0,
            CurrencyCode = senderWallet.CurrencyCode,
            Destination = $"WA-{receiverWallet.WalletNumber}",
            HasCommission = false,
            OperationType = OperationType.Cashback,
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
                                                   Transaction transaction)
    {
        try
        {

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
                TotalPricingAmount = transaction.Amount,
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

    private async Task SendP2PMoneyTransferAuditLogAsync(bool isSuccess, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = isSuccess,
                   LogDate = DateTime.Now,
                   Operation = "Cashback",
                   SourceApplication = "Emoney",
                   Resource = "MoneyTransfer",
                   UserId = Guid.Empty,
                   Details = details
               }
           );
    }
}
