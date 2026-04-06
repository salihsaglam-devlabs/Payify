using AutoMapper;
using LinkPara.Audit;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.AccountServiceProviders;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.CreatePaymentOrder;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendGkdNotification;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendOtpMessage;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetAccountTransactions;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalance;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.PaymentOrderInquiry;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Services.Secrets;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using MassTransit.Initializers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static MassTransit.ValidationResultExtensions;
using Transaction = LinkPara.Emoney.Domain.Entities.Transaction;



namespace LinkPara.Emoney.Infrastructure.Services;

public class OpenBankingService : IOpenBankingService
{

    private readonly ILogger<AccountService> _logger;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IConsentOperationsService _consentOperationsService;
    private readonly IGenericRepository<ChangedBalanceLog> _changedBalanceLogRepository;
    private readonly ITransferService _transferService;
    private readonly IIbanBlacklistService _ibanBlacklistService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<PaymentOrder> _paymentOrderRepository;

    private readonly string HhsCode;

    public OpenBankingService(
       ILogger<AccountService> logger,
       IServiceScopeFactory scopeFactory,
       IAuditLogService auditLogService,
       IMapper mapper,
       IGenericRepository<Transaction> transactionRepository,
       IGenericRepository<Wallet> walletRepository,
       INotificationService notificationService,
       IGenericRepository<Account> accountRepository,
       IUserService userService,
       IPushNotificationSender pushNotificationSender,
       IConsentOperationsService consentOperationsService,
       IGenericRepository<ChangedBalanceLog> changedBalanceLogRepository,
       ITransferService transferService,
       IIbanBlacklistService ibanBlacklistService,
       IApplicationUserService applicationUserService,
       IGenericRepository<PaymentOrder> paymentOrderRepository,
       ISecretService secretService)
    {
        _logger = logger;
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _notificationService = notificationService;
        _accountRepository = accountRepository;
        _userService = userService;
        _pushNotificationSender = pushNotificationSender;
        _consentOperationsService = consentOperationsService;
        _changedBalanceLogRepository = changedBalanceLogRepository;
        HhsCode = secretService.OpenBankingHhsSettings.HhsCode;
        _transferService = transferService;
        _ibanBlacklistService = ibanBlacklistService;
        _applicationUserService = applicationUserService;
        _paymentOrderRepository = paymentOrderRepository;
    }


    public async Task<AccountTransactionsDto> GetAccountTransactionsAsync(GetAccountTransactionsQuery query)
    {
        var walletId = await _walletRepository.GetAll()
                        .Where(a => a.AccountId == query.AccountId
                         && a.WalletNumber == query.AccountRef && a.RecordStatus == RecordStatus.Active)
                        .Select(a => a.Id).FirstOrDefaultAsync();

        if (walletId == Guid.Empty)
        {
            throw new NotFoundException(nameof(Transaction), query.AccountRef);
        }

        var transactionList = _transactionRepository.GetAll()
                                .Where(b => walletId == b.WalletId).AsQueryable();

        if (!transactionList.Any())
        {
            throw new NotFoundException(nameof(Transaction), query.AccountRef);
        }

        if (!string.IsNullOrEmpty(query.TransactionStartTime))
        {
            transactionList = transactionList.Where(b => b.TransactionDate
                                   >= DateTime.Parse(query.TransactionStartTime));
        }

        if (!string.IsNullOrEmpty(query.TransactionEndTime))
        {
            transactionList = transactionList.Where(b => b.TransactionDate
                                   <= DateTime.Parse(query.TransactionEndTime));
        }

        if (!string.IsNullOrEmpty(query.MinTransactionAmount))
        {
            transactionList = transactionList.Where(b => b.Amount
                                   >= decimal.Parse(query.MinTransactionAmount));
        }

        if (!string.IsNullOrEmpty(query.MaxTransactionAmount))
        {
            transactionList = transactionList.Where(b => b.Amount
                                   >= decimal.Parse(query.MaxTransactionAmount));
        }

        if (!string.IsNullOrEmpty(query.TransactionType) && query.TransactionType != "2")
        {
            transactionList = transactionList.Where(b => b.TransactionDirection.ToString()
                                   == query.TransactionType);
        }

        if (string.IsNullOrEmpty(query.SortBy))
        {
            query.SortBy = "TransactionDate";
        }

        if (String.IsNullOrEmpty(query.Page) || Convert.ToInt32(query.Page) == 0)
        {
            query.Page = "1";
        }

        if (String.IsNullOrEmpty(query.Size) || Convert.ToInt32(query.Size) == 0)
        {
            query.Size = "10";
        }

        var paginatedTxnList = await transactionList
            .PaginatedListAsync(Convert.ToInt32(query.Page), Convert.ToInt32(query.Size), query.OrderBy == "A" ? OrderByStatus.Desc : OrderByStatus.Asc, query.SortBy);

        var resultList = MapAccountTransactionResult(query, paginatedTxnList);

        var result = new AccountTransactionsDto()
        {
            TotalRecord = paginatedTxnList.TotalCount.ToString(),
            TransactionList = resultList
        };

        return result;
    }

    private List<AccTranDto> MapAccountTransactionResult(GetAccountTransactionsQuery request, PaginatedList<Transaction> txnList)
    {
        List<AccTranDto> resultList = new List<AccTranDto>();

        foreach (var transaction in txnList.Items)
        {
            var resultOb = new AccTranDto()
            {
                AccountRef = request.AccountRef,
                InstanceId = transaction.Id.ToString(),
                TranRefNo = transaction.WalletId.ToString(),
                TranAmount = transaction.Amount.ToString(),
                Currency = transaction.CurrencyCode,
                TranTime = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ssXXX"),
                Channel = transaction.Channel == "web" ? "I" : (transaction.Channel == "mobile" ? "M" : "D"),
                DebitOrCredit = transaction.TransactionDirection == TransactionDirection.MoneyIn ? "A" : "B",
                TransactionType = GetAccountTransactionType(transaction.TransactionType),
                TransactionPurpose = String.Empty,
                PaymentStmNo = "",
                Explanation = transaction.Description,
                OtherMaskedIban = "",
                OtherTitle = transaction.TransactionDirection == TransactionDirection.MoneyIn ? transaction.SenderName : transaction.ReceiverName
            };

            resultList.Add(resultOb);
        }

        return resultList;
    }

    private string GetAccountTransactionType(TransactionType transactionType)
    {
        var result = "DIGER";

        switch (transactionType)
        {
            case TransactionType.Withdraw:
                result = "PARA_YATIRMA";
                break;
            case TransactionType.Deposit:
                result = "PARA_CEKME";
                break;
            case TransactionType.Billing:
                result = "KURUM_FATURA_ODEMESI";
                break;
            case TransactionType.Commission:
                result = "UCRET_KOMISYON_FAIZ";
                break;
            case TransactionType.Tax:
                result = "VERGI_ODEMESI";
                break;
            case TransactionType.Epin:
            case TransactionType.IWallet:
            case TransactionType.Cashback:
            case TransactionType.Return:
            case TransactionType.BankReturn:
            case TransactionType.OnUs:
            default:
                break;
        }

        return result;
    }

    public async Task<List<ChangedBalanceDto>> GetChangedBalanceAsync(GetChangedBalanceQuery query)
    {
        return await _changedBalanceLogRepository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active && x.HasBalanceChanged)
            .Include(x => x.Wallet)
            .Select(x => new ChangedBalanceDto
            {
                Id = x.ConsentId,
                Ref = x.Wallet.WalletNumber,
                Time = x.LastEventTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();
    }

    public async Task<SendNotificationResultDto> SendGkdNotificationAsync(SendGkdNotificationCommand command)
    {

        var response = new SendNotificationResultDto();

        try
        {
            var userRequest = GenerateGkdNotificationRequest(command);


            var users = await _userService.GetAllUsersAsync(userRequest);

            var notificationUsers = users.Items.Select(x =>
            {
                return new NotificationUserInfo
                {
                    UserId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName
                };
            })
           .ToList();

            var userDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
            {
                UserIdList = notificationUsers.Select(x => x.UserId).ToList(),
            });

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("tr-TR");

            var receiverNotificationRequest = new SendPushNotification
            {
                TemplateName = "DiscreteGkdNotification",
                TemplateParameters = new Dictionary<string, string>
                 {
                   { "notificationContent", command.MessageContentTR},
                   { "deepLink", command.DeepLink }
                 },
                Tokens = userDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = notificationUsers
            };

            await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Send Push Notification Error : {exception.Message}", exception);
            response.IsSuccess = false;
        }

        response.IsSuccess = true;
        return response;
    }

    private GetUsersRequest GenerateGkdNotificationRequest(SendGkdNotificationCommand command)
    {
        var request = new GetUsersRequest();

        switch (command.CorporateIdentityType)
        {
            case "GSM":
                request.PhoneNumber = command.DecoupledIdValue;
                break;
            case "TCKN":
            case "YKN":
                request.IdentityNumber = command.DecoupledIdValue;
                break;
        }

        return request;
    }

    public async Task<SendOtpMessageResultDto> SendOtpMessageAsync(SendOtpMessageCommand command)
    {
        var response = new SendOtpMessageResultDto();

        var customer = await _accountRepository.GetAll()
                        .FirstOrDefaultAsync(a => a.Id == command.AccountId
                            && a.RecordStatus == RecordStatus.Active);

        if (customer is null)
        {
            throw new NotFoundException();
        }

        var phoneNumbers = new List<string>() { $"{customer.PhoneCode.Substring(1)}{customer.PhoneNumber}" };

        var request = new SmsRequest()
        {
            TemplateName = "PaymentOrderNotification",
            TemplateParameters = new Dictionary<string, string> { { "message", command.SmsContent } },
            To = phoneNumbers.ToArray()
        };
        var result = _notificationService.SendSmsNotificationAsync(request);

        response.IsSuccess = result != null && result.Result != null && result.Result.Success;

        return response;
    }

    public async Task<PaymentContractDto> CreatePaymentOrderAsync(CreatePaymentOrderCommand command, CancellationToken cancellationToken)
    {
        var isDuplicate = await _paymentOrderRepository.GetAll().AnyAsync(x => x.ConsentNumber == command.Contract.RzBlg.RizaNo);

        if (isDuplicate)
        {
            throw new DuplicateRecordException();
        }
        var newPaymentOrder = new PaymentOrder
        {
            ConsentNumber = command.Contract.RzBlg.RizaNo,
            ConsentCreateTime = DateTime.Parse(command.Contract.RzBlg.OlusZmn),
            YosCode = command.Contract.KatilimciBlg.YosCode,
            Amount = decimal.Parse(command.Contract.OdmBsltm.IslTtr.Ttr),
            CurrencyCode = command.Contract.OdmBsltm.IslTtr.PrBrm,
            CreateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            ReceiverIban = command.Contract.OdmBsltm.Alc.HspNo,
            ReceiverWalletNumber = command.Contract.OdmBsltm.Alc.HspRef,
            ReceiverTitle = command.Contract.OdmBsltm.Alc.Unv,
            SenderTitle = command.Contract.OdmBsltm.Gon.Unv,
            SenderWalletNumber = command.Contract.OdmBsltm.Gon.HspRef,
        };

        var result = new PaymentContractDto(command.Contract.RzBlg, command.Contract.KatilimciBlg, command.Contract.Gkd, command.Contract.OdmBsltm, command.Contract.IsyOdmBlg);

        try
        {
            if (command.Contract.KatilimciBlg.HhsCode != HhsCode)
            {
                throw new InvalidOperationException("HhsCode is not valid");
            }

            var senderWallet = await _walletRepository.GetAll()
                .Where(a => a.WalletNumber == command.Contract.OdmBsltm.Gon.HspRef
                             && a.RecordStatus == RecordStatus.Active)
                    .Include(x => x.Account)
                    .FirstOrDefaultAsync();

            if (senderWallet is null)
            {
                throw new NotFoundException();
            }

            if (senderWallet.Account.Name != command.Contract.OdmBsltm.Gon.Unv)
            {
                throw new InvalidOperationException("Account name and Unv is not match");
            }

            if (command.Contract.KatilimciBlg.YosCode == HhsCode)
            {
                var receiverWallet = await _walletRepository.GetAll()
                    .Where(a => a.WalletNumber == command.Contract.OdmBsltm.Alc.HspRef
                             && a.RecordStatus == RecordStatus.Active)
                    .Include(x => x.Account)
                    .FirstOrDefaultAsync();

                if (receiverWallet is null)
                {
                    throw new NotFoundException();
                }

                if (receiverWallet.Account.Name != command.Contract.OdmBsltm.Alc.Unv)
                {
                    throw new InvalidOperationException("Account name and Unv is not match");
                }

                if (!decimal.TryParse(command.Contract.OdmBsltm.IslTtr.Ttr, out var amount))
                {
                    throw new InvalidCastException();
                }

                var newTransferRequest = new TransferCommand
                {
                    Amount = amount,
                    Description = command.Contract.OdmBsltm.OdmAyr.OdmAcklm,
                    ReceiverWalletNumber = receiverWallet.WalletNumber,
                    SenderWalletNumber = senderWallet.WalletNumber,
                    UserId = _applicationUserService.ApplicationUserId.ToString()
                };
                var transferResponse = await _transferService.TransferAsync(newTransferRequest, cancellationToken);
                newPaymentOrder.IsSuccess = transferResponse.Success;
                newPaymentOrder.TransactionId = transferResponse.TransactionId;
            }
            else
            {
                if (!decimal.TryParse(command.Contract.OdmBsltm.IslTtr.Ttr, out var amount))
                {
                    throw new InvalidCastException();
                }

                var newWithdrawRequest = new WithdrawRequestCommand
                {
                    Amount = amount,
                    Description = command.Contract.OdmBsltm.OdmAyr.OdmAcklm,
                    ReceiverIBAN = command.Contract.OdmBsltm.Alc.HspNo,
                    ReceiverName = command.Contract.OdmBsltm.Alc.Unv,
                    WalletNumber = command.Contract.OdmBsltm.Gon.HspRef,
                    UserId = _applicationUserService.ApplicationUserId
                };

                var ibanBlacklisted = await _ibanBlacklistService.IsBlacklistedAsync(newWithdrawRequest.ReceiverIBAN);

                if (ibanBlacklisted)
                {
                    throw new IbanBlacklistedException(newWithdrawRequest.ReceiverIBAN);
                }

                var withdraw = await _transferService.WithdrawAsync(newWithdrawRequest, cancellationToken);

                newPaymentOrder.IsSuccess = withdraw.Success;
                newPaymentOrder.TransactionId = withdraw.TransactionId;
            }

        }
        catch (Exception exception)
        {
            _logger.LogError($"Error on CreatePaymentOrderAsync : {exception}");
            newPaymentOrder.IsSuccess = false;
            await _paymentOrderRepository.AddAsync(newPaymentOrder);
            throw;
        }
        await _paymentOrderRepository.AddAsync(newPaymentOrder);

        if (!newPaymentOrder.IsSuccess)
        {
            throw new InvalidOperationException();
        }

        result.OdmBsltm.OdmAyr.RefBlg = newPaymentOrder.Id.ToString();
        return result;
    }

    public async Task<PaymentContractDto> PaymentOrderInquiryAsync(PaymentOrderInquiryQuery query)
    {

        var paymentOrder = await _paymentOrderRepository.GetAll().Where(x => x.Id.ToString() == query.PaymentGuid).FirstOrDefaultAsync();
        if (paymentOrder is null)
        {
            throw new NotFoundException();
        }
        var result = new PaymentContractDto
        {
            RzBlg = new PaymentConsentDto
            {
                RizaNo = paymentOrder.ConsentNumber,
                OlusZmn = paymentOrder.ConsentCreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                RizaDrm = "K"
            },
            EmrBlg = new PaymentOrderInfoDto
            {
                OdmEmriNo = paymentOrder.Id.ToString(),
                OdmEmriZmn = paymentOrder.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
            },
            KatilimciBlg = new PaymentParticipantDto
            {
                HhsCode = HhsCode,
                YosCode = paymentOrder.YosCode
            },
            OdmBsltm = new PaymentInformationDto
            {
                IslTtr = new PriceInfo
                {
                    Ttr = paymentOrder.Amount.ToString(),
                    PrBrm = paymentOrder.CurrencyCode
                },
                Gon = new PersonInfo
                {
                    Unv = paymentOrder.SenderTitle,
                    HspRef = paymentOrder.SenderWalletNumber
                },
                Alc = new PersonInfo
                {
                    Unv = paymentOrder.ReceiverTitle,
                    HspRef = paymentOrder.ReceiverWalletNumber
                },
                OdmAyr = new PaymentDetail
                {
                }
            }
        };
        return result;
    }

    public async Task CheckChangedBalanceAsync()
    {
        try
        {
            var checkedReferences = await _consentOperationsService.GetConsentsForChangeBalanceControl();

            var logs = await _changedBalanceLogRepository.GetAll().ToListAsync();

            logs.ForEach(log =>
            {
                log.RecordStatus = RecordStatus.Passive;
                log.HasBalanceChanged = false;
            });

            await _changedBalanceLogRepository.UpdateRangeAsync(logs);

            foreach (var reference in checkedReferences)
            {
                bool hasBalance = await _changedBalanceLogRepository.GetAll().Include(x => x.Wallet).AnyAsync(x => x.Wallet.WalletNumber == reference.Ref);

                if (hasBalance)
                {
                    var log = await _changedBalanceLogRepository.GetAll().Include(x => x.Wallet).FirstOrDefaultAsync(x => x.Wallet.WalletNumber == reference.Ref);

                    log.ConsentId = reference.Id;
                    log.RecordStatus = RecordStatus.Active;

                    await CheckLastFiveMinutesBalanceChangedAsync(log);

                    await _changedBalanceLogRepository.UpdateAsync(log);
                }
                else
                {
                    var wallet = await _walletRepository.GetAll().Where(x => x.WalletNumber == reference.Ref).FirstOrDefaultAsync();
                    var changedBalanceLog = new ChangedBalanceLog
                    {
                        ConsentId = reference.Id,
                        WalletId = wallet.Id,
                    };

                    await CheckLastFiveMinutesBalanceChangedAsync(changedBalanceLog);
                    await _changedBalanceLogRepository.AddAsync(changedBalanceLog);
                }

            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error on CheckChangedBalanceAsync : {exception}");
        }
    }

    private async Task CheckLastFiveMinutesBalanceChangedAsync(ChangedBalanceLog changedBalance)
    {
        var transactions = await _transactionRepository.GetAll()
            .Include(x => x.Wallet)
            .Where(x => x.Wallet.WalletNumber == changedBalance.Wallet.WalletNumber && x.TransactionDate > DateTime.Now.AddMinutes(-5))
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new { x.TransactionDate, x.CurrentBalance })
            .ToListAsync();

        if (transactions is null)
        {
            return;
        }

        var firtTransaction = transactions.FirstOrDefault();
        var lastTransaction = transactions.LastOrDefault();

        if (firtTransaction.CurrentBalance != lastTransaction.CurrentBalance)
        {
            changedBalance.HasBalanceChanged = true;
            changedBalance.LastEventTime = firtTransaction.TransactionDate;
        }
    }
}
