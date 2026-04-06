using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Events;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Identity.Application.Features.Users.EventHandlers;

public class UserCreatedEventHandler : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    private readonly IEmailSender _emailSender;
    private readonly IVaultClient _vaultClient;
    private readonly IUserEmailService _userEmailService;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;

    public UserCreatedEventHandler(IEmailSender emailSender,
                                    IVaultClient vaultClient,
                                    IUserEmailService userEmailService,
                                    IParameterService parameterService,
                                    IBus bus)
    {
        _emailSender = emailSender;
        _vaultClient = vaultClient;
        _userEmailService = userEmailService;
        _parameterService = parameterService;
        _bus = bus;
    }

    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var user = domainEvent.User;

        switch (user.UserType)
        {
            case UserType.Individual:
                break;
            case UserType.Corporate:
            case UserType.CorporateSubMerchant:
                await SendCorporateUserOnboardingMailAsync(domainEvent.User, domainEvent.Parameters);
                break;
            case UserType.Internal:
            case UserType.Representative:
                await SendInternalUserOnboardingMailAsync(domainEvent.User, domainEvent.Parameters);
                break;
            case UserType.Branch:
                await SendInternalUserOnboardingMailAsync(domainEvent.User, domainEvent.Parameters);
                break;
            case UserType.ApplicationUser:
                break;
            case UserType.CorporateWallet:
                await SendCorporateWalletUserOnboardingMailAsync(domainEvent.User, domainEvent.Parameters);
                break;
        }
    }

    private async Task SendCorporateWalletUserOnboardingMailAsync(User user, Dictionary<string, string> parameters)
    {
        var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "CorporateWallet")}/account/reset-password?token={parameters["ResetPasswordToken"]}&username={user.UserName}";

        await _bus.Publish(new UserOnboarding
        {
            UserId = user.Id,
            Link = link,
            Username = $"{user.PhoneCode}{user.PhoneNumber}"
        }, CancellationToken.None);
    }

    private async Task SendInternalUserOnboardingMailAsync(User user, Dictionary<string, string> mailParameters)
    {
        var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Backoffice")}/account/reset-password?token={mailParameters["ResetPasswordToken"]}&username={user.UserName}";

        await _bus.Publish(new UserOnboarding
        {
            UserId = user.Id,
            Link = link,
            Username = $"{user.UserName}"
        }, CancellationToken.None);
    }

    private async Task SendCorporateUserOnboardingMailAsync(User user, Dictionary<string, string> mailParameters)
    {
        var emailConfirmationEnabled = false;
        try
        {
            var parameter = await _parameterService.GetParameterAsync("IdentityParameters", "EmailConfirmationEnabled");
            emailConfirmationEnabled = bool.Parse(parameter.ParameterValue);
        }
        catch
        {
        }
        if (emailConfirmationEnabled)
        {
            await _userEmailService.SendEmailVerificationMailAsync(user);
            return;
        }

        await _userEmailService.SendCorporateUserOnboardingMailAsync(user, mailParameters);

    }
}