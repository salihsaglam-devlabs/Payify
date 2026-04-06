using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Commands.CallCenterCustomerConfirmation;
public class CallCenterCustomerConfirmationCommand : IRequest<CustomerConfirmationResponse>
{
    public string PhoneNumber { get; set; }
}

public class CallCenterCustomerConfirmationCommandHandler : IRequestHandler<CallCenterCustomerConfirmationCommand, CustomerConfirmationResponse>
{
    private readonly IUserService _userService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<CallCenterNotificationLog> _callcenterRepository;
    private readonly IPushNotificationSender _pusNotificationSender;
    private readonly IContextProvider _contextProvider;

    public CallCenterCustomerConfirmationCommandHandler(
        IUserService userService, 
        IGenericRepository<AccountUser> accountUserRepository, 
        IPushNotificationSender pusNotificationSender, 
        IContextProvider contextProvider, 
        IGenericRepository<CallCenterNotificationLog> callcenterRepository)
    {
        _userService = userService;
        _accountUserRepository = accountUserRepository;
        _pusNotificationSender = pusNotificationSender;
        _contextProvider = contextProvider;
        _callcenterRepository = callcenterRepository;
    }

    public async Task<CustomerConfirmationResponse> Handle(CallCenterCustomerConfirmationCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CustomerConfirmationResponse();

        var user = await GetUserByPhone(request.PhoneNumber);

        if (user is null)
        {
            throw new NotFoundException("PhoneNumber", request.PhoneNumber);
        }

        var log = new CallCenterNotificationLog()
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = request.PhoneNumber,
            ConfirmationType = CallCenterConfirmationType.PushNotification,
            Status = CallCenterNotificationStatus.Pending,
            ExpireDate = DateTime.Now.AddMinutes(10),
            CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
            RecordStatus = RecordStatus.Active
        };

        await _callcenterRepository.AddAsync(log);

        var account = await GetAccount(user.Id);
        
        try
        {
            await PushSenderNotificationAsync(account, log); 
        }
        catch (Exception e)
        {
            log.Status = CallCenterNotificationStatus.Error;
            log.ErrorMessage = e.Message?.ToString();

            await _callcenterRepository.UpdateAsync(log);
            response.IsSuccess = false;
            return response;
        }

        response.IsSuccess = true;
        response.AccountId= account.Id;
        response.PushNotificationId = log.Id;

        return response;
    }

    private async Task PushSenderNotificationAsync( Account account,
                                                    CallCenterNotificationLog notificationLog)
    {
        var userList = account.AccountUsers
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

        var userDeviceInfo = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = userList.Select(x => x.UserId).ToList(),
        });

        if (userDeviceInfo != null && userDeviceInfo.Count > 0)
        {
            var senderNotificationRequest = new SendPushNotification
            {
                TemplateName = "CallCenterConfirmationNotification",
                TemplateParameters = new Dictionary<string, string>(),
                Tokens = userDeviceInfo.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = userList,
                Data = new Dictionary<string, string>
            {
                { "CallCenterNotificationId" , notificationLog.Id.ToString() },
                { "ExpireDate" , notificationLog.ExpireDate.ToString() }
            }
            };

            await _pusNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
        }
        else
        {
            throw new NotFoundException(nameof(UserDeviceInfoDto), userList.Select(x => x.UserId).FirstOrDefault());
        }
        
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

    private async Task<Account> GetAccount(Guid userId)
    {
        var accountUser = await _accountUserRepository
                .GetAll()
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.UserId == userId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), userId);
        }

        if (accountUser.Account is null)
        {
            throw new NotFoundException(nameof(Account), accountUser.AccountId);
        }

        return accountUser.Account;
    }
}
