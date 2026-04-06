using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace LinkPara.ApiGateway.Merchant.Authorization.CustomAuthorizations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IAuthHttpClient _authHttpClient;
    private readonly IStringLocalizer _localizer;
    private readonly IVaultClient _vaultClient;
    private readonly ICacheService _cacheService;

    public AuthorizeAttribute(IAuthHttpClient authHttpClient,
        IStringLocalizerFactory factory,
        IVaultClient vaultClient,
        ICacheService cacheService)
    {
        _authHttpClient = authHttpClient;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.Merchant");
        _vaultClient = vaultClient;
        _cacheService = cacheService;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userIdentity = context.HttpContext!.User.Identity;

        if (userIdentity is not { IsAuthenticated: true })
            return;

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment?.ToLowerInvariant() == "development")
        {
            return;
        }

        var userType
            = context.HttpContext.User.FindFirst(q => q.Type == ClaimKey.UserType);

        if (userType is null)
        {
            Reject(context);
            return;
        }

        var channel = context.HttpContext.Request.Headers["Channel"].ToString();

        var validLogin = channel == ChannelType.MerchantPortal.ToString() &&
                         (userType.Value == Enum.GetName(UserType.Corporate) ||
                          userType.Value == Enum.GetName(UserType.CorporateSubMerchant));

        if (!validLogin)
        {
            Reject(context);
        }

        var userSessionId = context.HttpContext.User.FindFirst(q => q.Type == ClaimKey.SessionId);

        var sessionControl = CheckSessionControlAsync();

        if (sessionControl)
        {
            if (userSessionId is null)
            {
                Reject(context);
                return;
            }

            try
            {
                var userSession = _authHttpClient.GetUserSessionAsync(userSessionId.Value).GetAwaiter().GetResult();

                if (userSession.RefreshTokenExpiration > DateTime.UtcNow &&
                    userSession.RecordStatus == RecordStatus.Passive)
                {
                    var message = _localizer.GetString("NewSessionOpenedException");
                    Reject(context, message);
                }
            }
            catch (Exception ex)
            {
                Reject(context);
            }
        }
    }

    private bool CheckSessionControlAsync()
    {
        var sessionControl = _cacheService.Get<bool?>("SessionControl");

        if (sessionControl is null)
        {
            var endOfDay = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            var timeRemaining = endOfDay - DateTime.Now;
            var minutesRemaining = (int)Math.Ceiling(timeRemaining.TotalMinutes);

            try
            {
                sessionControl = _vaultClient
                    .GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "MerchantSessionControlEnabled")
                    .GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                sessionControl = false;
            }

            _cacheService.Add("SessionControl", sessionControl, minutesRemaining);
        }

        return sessionControl ?? false;
    }

    private static void Reject(AuthorizationFilterContext context, string message = null)
    {
        var details = new ProblemDetails
        {
            Title = message is not null ? message : "Unauthorized",
            Detail = "Unauthorized user type access!",
            Status = StatusCodes.Status401Unauthorized
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized,
        };
    }
}