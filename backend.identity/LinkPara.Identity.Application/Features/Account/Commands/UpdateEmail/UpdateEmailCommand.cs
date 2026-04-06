using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Account.Commands.UpdateEmail;
public class UpdateEmailCommand : IRequest
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string NewEmail { get; set; }
}

public class UpdateEmailCommandHandler : IRequestHandler<UpdateEmailCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UpdateEmailCommandHandler> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IRepository<User> _userRepository;
    private readonly IBus _bus;

    public UpdateEmailCommandHandler(UserManager<User> userManager,
        IDeviceInfoService deviceInfoService,
        IPushNotificationSender pushNotificationSender,
        INotificationService notificationService,
        ILogger<UpdateEmailCommandHandler> logger,
        IEmailSender emailSender,
        IRepository<User> userRepository,
        IBus bus)
    {
        _userManager = userManager;
        _deviceInfoService = deviceInfoService;
        _pushNotificationSender = pushNotificationSender;
        _notificationService = notificationService;
        _logger = logger;
        _emailSender = emailSender;
        _userRepository = userRepository;
        _bus = bus;
    }

    public async Task<Unit> Handle(UpdateEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);

        if (user is null)
        {
            throw new NotFoundException(nameof(user));
        }

        var oldMail = user.Email;

        var userWithSameMail = await _userRepository
            .GetAll()
            .Select(x => new
            {
                x.Email,
                x.UserStatus,
                x.UserType
            })
            .FirstOrDefaultAsync(x =>
                x.Email == command.NewEmail.ToLower()
                && (x.UserStatus == UserStatus.Active || x.UserStatus == UserStatus.Suspended)
                && x.UserType == user.UserType, cancellationToken);
        
        if (userWithSameMail is not null)
        {
            throw new AlreadyInUseException(nameof(command.NewEmail));
        }

        var currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        
        var result = await _userManager.ChangeEmailAsync(user, command.NewEmail.ToLower(), Base64UrlEncoder.Decode(command.Token));
 
        if (!result.Succeeded)
        {
            _logger.LogError($"UpdateEmail Error : {string.Join(",", result.Errors.Select(s => s.Description).ToList())}");
 
            throw new InvalidNewEmailException();
        }
 
        var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
 
        await _userManager.ConfirmEmailAsync(user, emailConfirmToken);
        
        await _bus.Publish(new EmailChanged
        {
            UserId = user.Id,
            NewValue = command.NewEmail,
            CurrentDate = currentDate
        });
        
        if (user.UserType == UserType.Individual)
        {
            await _notificationService.SendAdvancedEmailNotificationAsync(new AdvancedEmailRequest
            {
                EventName = "Identity.EmailChanged",
                PreferredLanguage = ContentLanguage.TR.ToString(),
                ReceiverId = user.Id,
                ToEmail = [oldMail],
                TemplateParameters = new Dictionary<string, string>
                {
                    { "Yeni Email", command.NewEmail },
                    { "Değiştirilme Tarihi", currentDate }
                }
            });
        }

        return Unit.Value;
    }

    private async Task SendPushNotification(User user, UpdateEmailCommand command)
    {
        var userDeviceInfo = await _deviceInfoService.GetUsersDeviceInfoAsync(new List<Guid>()
                {
                    user.Id
                });
        var userEmailChangeNotificationRequest = new SendPushNotification
        {
            TemplateName = "EmailChange",
            TemplateParameters = new Dictionary<string, string>
            {
                { "newValue", command.NewEmail },
                { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
            },
            Tokens = userDeviceInfo.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
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

        await _pushNotificationSender.SendPushNotificationAsync(userEmailChangeNotificationRequest);
    }
}