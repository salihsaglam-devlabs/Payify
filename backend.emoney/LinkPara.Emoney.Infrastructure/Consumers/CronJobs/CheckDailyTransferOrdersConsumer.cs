using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckDailyTransferOrdersConsumer : IConsumer<CheckDailyTransferOrders>
{
    private readonly ILogger<CheckDailyTransferOrdersConsumer> _logger;
    private readonly IGenericRepository<TransferOrder> _repository;
    private readonly ITransferService _moneyTransferService;
    private readonly EmoneyDbContext _dbContext;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IUserService _userService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IStringLocalizer _localizer;
    private const string PaymentFailed = "PaymentFailed";
    private const string InsufficientBalance = "InsufficientBalance";
    private const string LimitExceeded = "LimitExceeded";

    public CheckDailyTransferOrdersConsumer(
        ILogger<CheckDailyTransferOrdersConsumer> logger,
        IGenericRepository<TransferOrder> repository,
        ITransferService moneyTransferService,
        EmoneyDbContext dbContext,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IUserService userService,
        IGenericRepository<AccountUser> accountUserRepository,
        IPushNotificationSender pushNotificationSender,
        IStringLocalizerFactory stringLocalizerFactory)
    {
        _logger = logger;
        _repository = repository;
        _moneyTransferService = moneyTransferService;
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
        _userService = userService;
        _accountUserRepository = accountUserRepository;
        _pushNotificationSender = pushNotificationSender;
        _localizer = stringLocalizerFactory.Create("Notifications", "LinkPara.Emoney.API");
    }

    public async Task Consume(ConsumeContext<CheckDailyTransferOrders> context)
    {
        var transferOrderList = await _repository
            .GetAll()
            .Where(w => w.RecordStatus == RecordStatus.Active
                     && w.TransferOrderStatus == TransferOrderStatus.Pending
                     && w.TransferDate.Date == DateTime.Now.Date)
            .ToListAsync();

        foreach (var order in transferOrderList)
        {
            MoneyTransferResponse bankResponse = null;

            if (order is null)
            {
                _logger.LogError($"CheckDailyTransferOrdersConsumer - Error : Order Not Found");
                return;
            }
            try
            {
                bankResponse = order.ReceiverAccountType switch
                {
                    ReceiverAccountType.Iban => await MoneyTransferToIBANAsync(order),
                    _ => await MoneyTransferToWalletAsync(await SetReceiverWalletAsync(order))
                };

                if (bankResponse.Success)
                {
                    MarkAsCompleted(order);
                    await TransferOrderAuditLogAsync(true, order.CreatedBy, new Dictionary<string, string>
                    {
                        {"TransferOrderId",order.Id.ToString()},
                        {"Message","TransferOrderIsSuccess" }
                    });
                }
                else
                {
                    MarkAsFailed(order, bankResponse);
                    await TransferOrderAuditLogAsync(false, order.CreatedBy, new Dictionary<string, string>
                    {
                        {"TransferOrderId",order.Id.ToString()},
                        {"Message","TransferOrderIsFailed" },
                        {"BankResponseErrorMessage",bankResponse.ErrorMessage }
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"CheckDailyTransferOrders Consumer Error {exception}", exception);

                MarkAsFailed(order, bankResponse);

                await TransferOrderAuditLogAsync(false, order.CreatedBy, new Dictionary<string, string>
                {
                    {"TransferOrderId",order.Id.ToString()},
                    {"ErrorMessage",exception.Message }
                });

                await _dbContext.SaveChangesAsync();
            }
        }
    }
    private async Task<TransferOrder> SetReceiverWalletAsync(TransferOrder order)
    {
        var receiverWalletNumber = order.ReceiverAccountType switch
        {
            ReceiverAccountType.PhoneNumber => await GetReceiverWalletByPhoneAsync(order.ReceiverPhoneCode, order.ReceiverAccountValue),
            _ => order.ReceiverAccountValue
        };

        _dbContext.Attach(order);
        order.ReceiverWalletNumber = receiverWalletNumber;
        order.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

        return order;
    }

    private async Task<string> GetReceiverWalletByPhoneAsync(string phoneCode, string receiverPhoneNumber)
    {
        UserDto user = new();
        var userName = await GetUserNameHelper.GetUserNameAsync(phoneCode, receiverPhoneNumber);

        var users = await _userService.GetAllUsersAsync(new GetUsersRequest { UserName = userName, UserStatus = UserStatus.Active });

        if (users.Items.Count != 1)
        {
            throw new NotFoundException(nameof(Wallet));
        }
        else if (users.Items.Count == 0)
        {
            throw new NotFoundException(receiverPhoneNumber);
        }
        else
        {
            user = users.Items[0];
        }

        var accountUser = await _accountUserRepository.GetAll()
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.RecordStatus == RecordStatus.Active);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser));
        }

        var wallets = await _dbContext.Wallet
            .Where(q => q.AccountId == accountUser.AccountId && q.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        var wallet = wallets.FirstOrDefault(q => q.IsMainWallet) ?? wallets.FirstOrDefault();

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet));
        }

        return wallet.WalletNumber;
    }

    private async Task MarkAsFailed(TransferOrder order, MoneyTransferResponse bankRespose)
    {
        if (order is not null)
        {
            _dbContext.Attach(order);
            order.TransferOrderStatus = TransferOrderStatus.Failed;
            order.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            order.ErrorMessage = !string.IsNullOrEmpty(bankRespose?.ErrorMessage) ? bankRespose.ErrorMessage : "CheckDailyTransferError";
        }

        if (bankRespose is not null)
        {
            _logger.LogError("Money Transfer Order Error {@walletTransfer}", bankRespose);
        }

        if (order is not null && bankRespose is not null)
        {
            await SendNotification(order.UserId, order.TransferDate, bankRespose.ErrorMessage);
        }
    }

    private void MarkAsCompleted(TransferOrder order)
    {
        _dbContext.Attach(order);
        order.TransferOrderStatus = TransferOrderStatus.Completed;
        order.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
    }

    private async Task<MoneyTransferResponse> MoneyTransferToWalletAsync(TransferOrder order)
    {
        try
        {
            var cancellation = new CancellationTokenRegistration();

            return await _moneyTransferService.TransferAsync(
                request: new TransferCommand
                {
                    Amount = order.Amount,
                    SenderWalletNumber = order.SenderWalletNumber,
                    ReceiverWalletNumber = order.ReceiverWalletNumber,
                    Description = order.Description,
                    UserId = order.UserId.ToString(),
                    PaymentType = order.PaymentType
                },
                cancellationToken: cancellation.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Money Transfer Order Error : {exception.Message}", exception.Message);

            return new MoneyTransferResponse
            {
                Success = false,
                ErrorMessage = exception.Message
            };
        }
    }

    private async Task<MoneyTransferResponse> MoneyTransferToIBANAsync(TransferOrder order)
    {
        try
        {
            var cancellation = new CancellationTokenRegistration();

            return await _moneyTransferService.WithdrawAsync(
                request: new WithdrawRequestCommand
                {
                    Amount = order.Amount,
                    Description = order.Description,
                    ReceiverIBAN = order.ReceiverAccountValue,
                    ReceiverName = order.ReceiverNameSurname,
                    UserId = order.UserId,
                    WalletNumber = order.SenderWalletNumber,
                    PaymentType = order.PaymentType
                },
                cancellationToken: cancellation.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Money Transfer Order To IBAN Error : {exception.Message}", exception.Message);

            return new MoneyTransferResponse
            {
                Success = false,
                ErrorMessage = exception.Message
            };
        }
    }

    private async Task TransferOrderAuditLogAsync(bool isSuccess, string userId, Dictionary<string, string> deatils)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            id = Guid.Empty;
        }
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "CheckDailyTransferOrders",
                Resource = "TransferOrder",
                SourceApplication = "Emoney",
                UserId = id
            }
        );
    }

    private async Task SendNotification(Guid userId, DateTime orderDate, string errorMessage)
    {
        var reason = errorMessage switch
        {
            InsufficientBalance => InsufficientBalance,
            LimitExceeded => LimitExceeded,
            _ => PaymentFailed
        };

        try
        {
            var user = await _userService.GetUserAsync(userId);

            if (user is null)
            {
                _logger.LogError($"Notification User Not Found Error : {userId}");
                return;
            }

            var users = await _userService.GetAllUsersAsync(new GetUsersRequest() { Email = user.Email });

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
                TemplateName = "FuturePaymentFailed",
                TemplateParameters = new Dictionary<string, string>
                {
                  { "date", orderDate.ToString("dd-MM-yyyy")},
                  { "reason",_localizer.GetString(reason).Value }
                },
                Tokens = userDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = notificationUsers
            };

            await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Send Push Notification Error : {exception.Message}", exception);
            return;
        }
    }
}