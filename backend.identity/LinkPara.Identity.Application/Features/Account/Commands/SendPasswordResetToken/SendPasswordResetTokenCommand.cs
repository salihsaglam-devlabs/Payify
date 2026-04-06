using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Commands.CreateUser;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.ContextProvider;
using LinkPara.Identity.Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using LinkPara.HttpProviders.Vault;
using MassTransit;

namespace LinkPara.Identity.Application.Features.Account.Commands.SendPasswordResetToken;

public class SendPasswordResetTokenCommand : IRequest<string>
{
    public string UserName { get; set; }
}

public class SendPasswordResetTokenCommandHandler : IRequestHandler<SendPasswordResetTokenCommand, string>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SendPasswordResetTokenCommandHandler> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IVaultClient _vaultClient;
    private readonly IBus _bus;

    public SendPasswordResetTokenCommandHandler(
        UserManager<User> userManager,
        ILogger<SendPasswordResetTokenCommandHandler> logger,
        IContextProvider contextProvider,
        IVaultClient vaultClient,
        IBus bus)
    {
        _userManager = userManager;
        _logger = logger;
        _contextProvider = contextProvider;
        _vaultClient = vaultClient;
        _bus = bus;
    }

    public async Task<string> Handle(SendPasswordResetTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.UserName);

        if (user is not null)
        {
            var webSiteUrl = GetWebsiteUrl(_contextProvider.CurrentContext.Gateway, user.UserType);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Base64UrlEncoder.Encode(token);
            await _bus.Publish(new SharedModels.Notification.NotificationModels.Identity.ResetPassword
            {
                UserId = user.Id,
                Link = $"{webSiteUrl}?token={encodedToken}&username={user.UserName}"
            }, cancellationToken);
            return encodedToken;
        }
        _logger.LogError($"User not found: {command.UserName}");
        return string.Empty;
    }
    private string GetWebsiteUrl(string gateway, UserType userType)
    {
        var webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Web")}/reset-password";

        if (!string.IsNullOrEmpty(gateway) && gateway == Gateway.BackOffice.ToString())
        {
            webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Backoffice")}/account/reset-password";
        }

        if (userType is UserType.Corporate or UserType.CorporateSubMerchant)
        {
            webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Merchant")}/account/reset-password";
        }

        if (userType == UserType.CorporateWallet)
        {
            webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "CorporateWallet")}/account/reset-password";
        }

        return webSiteUrl;
    }
}