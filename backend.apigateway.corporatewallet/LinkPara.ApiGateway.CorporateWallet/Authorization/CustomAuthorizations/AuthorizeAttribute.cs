using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Authorization.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinkPara.ApiGateway.CorporateWallet.Authorization.CustomAuthorizations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
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

        var validLogin = channel == ChannelType.CorporateWalletPortal.ToString() && userType.Value == Enum.GetName(UserType.Corporate);

        if (!validLogin)
        {
            Reject(context);
        }
    }

    private static void Reject(AuthorizationFilterContext context)
    {
        var details = new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = "Unauthorized user type access!",
            Status = StatusCodes.Status401Unauthorized
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized,
        };
    }
}