using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;

namespace LinkPara.Identity.Infrastructure.Services;

public class UserLoginService : IUserLoginService
{
    private readonly IBus _eventBus;
    private readonly ILogger<UserLoginLog> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IRepository<UserLoginLastActivity> _userLoginLastActivity;
    private readonly IRepository<LoginActivity> _loginActivity;
    private readonly IVaultClient _vaultClient;
    private readonly IEmailSender _emailSender;
    private readonly IContextProvider _contextProvider;

    public UserLoginService(IHttpContextAccessor contextAccessor
        , IBus eventBus
        , ILogger<UserLoginLog> logger
        , IRepository<UserLoginLastActivity> userLoginLastActivity,
        IRepository<LoginActivity> loginActivity,
        IVaultClient vaultClient,
        IEmailSender emailSender,
        IContextProvider contextProvider)
    {
        _contextAccessor = contextAccessor;
        _eventBus = eventBus;
        _logger = logger;
        _userLoginLastActivity = userLoginLastActivity;
        _loginActivity = loginActivity;
        _vaultClient = vaultClient;
        _emailSender = emailSender;
        _contextProvider = contextProvider;
    }

    public async Task SaveLoginInfoAsync(User user, SignInResult result)
    {
        if (user is not null)
        {
            try
            {
                var loginResultCode = ConvertToLoginResult(result);
                await SaveUserLastActivityAsync(loginResultCode, user.Id, user);
                await SaveUserLoginActivityAsync(loginResultCode, user.Id);

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                var endpoint = await _eventBus.GetSendEndpoint(new Uri("exchange:Log.UserLogin"));
                await endpoint.Send(new UserLoginLog
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email,
                    RemoteIpAddress = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    LocaleIpAddress = _contextAccessor.HttpContext?.Connection?.LocalIpAddress?.ToString(),
                    UserAgent = _contextAccessor.HttpContext?.Request.Headers["User-Agent"],
                    LoginResultCode = loginResultCode,
                    LoginResult = loginResultCode.ToString(),
                    CreateDate = DateTime.Now
                }, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Request Response Logging Publish Error : {exception}");
            }
        }
    }

    private async Task SaveUserLoginActivityAsync(LoginResult loginResultCode, Guid userId)
    {
        try
        {
            var ip = _contextProvider.CurrentContext.ClientIpAddress;

            if (!string.IsNullOrEmpty(ip) && !IsValidIP(ip))
            {
                _logger.LogError($"IP address is not valid {ip}");
                ip = string.Empty;
            }

            var port = _contextProvider.CurrentContext.Port;

            if (!int.TryParse(port, out int result))
            {
                port = "0";
            }

            var loginActivity = new LoginActivity
            {
                Date = DateTime.Now,
                LoginResult = loginResultCode == LoginResult.Succeeded ? loginResultCode : LoginResult.Failed,
                UserId = userId,
                CreatedBy = userId.ToString(),
                IP = ip,
                Port = port,
                Channel = _contextProvider.CurrentContext.Channel
            };

            await _loginActivity.AddAsync(loginActivity);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveLoginActivityError detail:{exception}");
        }
    }

    private LoginResult ConvertToLoginResult(SignInResult signInResult)
    {
        if (signInResult.Succeeded)
        {
            return LoginResult.Succeeded;
        }

        if (signInResult.IsLockedOut)
        {
            return LoginResult.IsLockedOut;
        }

        if (signInResult.RequiresTwoFactor)
        {
            return LoginResult.RequiresTwoFactor;
        }

        if (signInResult.IsNotAllowed)
        {
            return LoginResult.IsNotAllowed;
        }

        return LoginResult.Failed;
    }
    private async Task SaveUserLastActivityAsync(LoginResult loginResult, Guid userId, User user)
    {
        try
        {
            var userLoginLastActivity = await GetUserLoginLastActivityAsync(userId);

            if (userLoginLastActivity == null)
            {
                userLoginLastActivity = new UserLoginLastActivity
                {
                    UserId = userId,
                    LoginResult = loginResult,
                    CreatedBy = userId.ToString(),
                    Channel = _contextProvider.CurrentContext.Channel
                };

                userLoginLastActivity = SetDateByLoginResult(loginResult, userLoginLastActivity);

                await _userLoginLastActivity.AddAsync(userLoginLastActivity);
            }
            else
            {
                userLoginLastActivity.LoginResult = loginResult;
                userLoginLastActivity = SetDateByLoginResult(loginResult, userLoginLastActivity);

                await _userLoginLastActivity.UpdateAsync(userLoginLastActivity);
            }

            var maxFailedAccessAttempts = _vaultClient.GetSecretValue<int>("IdentitySecrets", "LockoutSettings", "MaxFailedAccessAttempts");

            if (userLoginLastActivity.FailedLoginCount == maxFailedAccessAttempts)
            {
                await SendEmailAsync(user);

                if (user.UserType == UserType.Individual)
                {
                    await SendPushNotification(user);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorOnSaveUserLoginLastActivity exceptionDetail: {exception}");
        }
    }

    private async Task<UserLoginLastActivity> GetUserLoginLastActivityAsync(Guid userId)
    {
        return await _userLoginLastActivity.GetAll()
            .SingleOrDefaultAsync(s => s.UserId == userId);
    }

    private UserLoginLastActivity SetDateByLoginResult(LoginResult loginResult, UserLoginLastActivity userLoginLastActivity)
    {
        switch (loginResult)
        {
            case LoginResult.Succeeded:
                userLoginLastActivity.LastSucceededLogin = DateTime.Now;
                userLoginLastActivity.FailedLoginCount = 0;
                break;
            case LoginResult.Failed:
                userLoginLastActivity.LastFailedLogin = DateTime.Now;
                userLoginLastActivity.FailedLoginCount = userLoginLastActivity.FailedLoginCount + 1;
                break;
            case LoginResult.IsLockedOut:
                userLoginLastActivity.LastLockedLogin = DateTime.Now;
                userLoginLastActivity.FailedLoginCount = userLoginLastActivity.FailedLoginCount + 1;
                break;
            default:
                break;
        }
        return userLoginLastActivity;
    }
    private async Task SendPushNotification(User user)
    {
        try
        {
            await _eventBus.Publish(new LockOutInformation
            {
                UserId = user.Id
            }, CancellationToken.None);
        }
        catch (Exception exception)
        {

            _logger.LogError($"SendNotificationError detail:{exception}");
        }
    }
    private async Task SendEmailAsync(User user)
    {
        var forgotPasswordLink = GetWebsiteUrl(user.UserType);

        var emailRequest = new SendEmail
        {
            ToEmail = user.Email,
            DynamicTemplateData = new()
            {
                { "link", forgotPasswordLink }
            }
        };

        var autoUnlock = _vaultClient.GetSecretValue<bool>("IdentitySecrets", "LockoutSettings", "AutoUnlock");

        if (autoUnlock)
        {
            emailRequest.TemplateName = "UserTemporaryLockedOut";
        }
        else
        {
            emailRequest.TemplateName = "UserPermanentLockedOut";
        }
        await _emailSender.SendEmailAsync(emailRequest);
    }

    private string GetWebsiteUrl(UserType userType)
    {
        var webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Web")}/forgot-password";

        if (userType == UserType.Internal)
        {
            webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Backoffice")}/account/forgot-password";
        }

        if (userType is UserType.Corporate or UserType.CorporateSubMerchant)
        {
            webSiteUrl = $"{_vaultClient.GetSecretValue<string>("/SharedSecrets", "ServiceUrls", "Merchant")}/account/forgot-password";
        }

        return webSiteUrl;
    }

    public static bool IsValidIP(string ipAddress)
    {
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        return Regex.IsMatch(ipAddress, pattern);
    }
}