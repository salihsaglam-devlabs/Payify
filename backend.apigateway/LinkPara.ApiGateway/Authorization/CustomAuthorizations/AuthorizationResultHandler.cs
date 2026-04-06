using LinkPara.ApiGateway.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Authorization.CustomAuthorizations;

public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler _handler;

    public AuthorizationResultHandler()
    {
        _handler = new AuthorizationMiddlewareResultHandler();
    }

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden
           && authorizeResult.AuthorizationFailure!.FailedRequirements
               .OfType<HasScopeRequirement>().Any())
        {
            var details = new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "Unauthorized user access request!",
                Status = StatusCodes.Status403Forbidden
            };

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(details);
            await context.Response.CompleteAsync();
        }

        if (authorizeResult.Challenged)
        {
            if (!context.Response.HasStarted)
            {
                var otpError = context.Items["OTPError"]?.ToString();

                if (otpError is not null)
                {
                    var details = new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = otpError,
                        Status = StatusCodes.Status401Unauthorized
                    };

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(details);
                    await context.Response.CompleteAsync();
                    return;
                }

            }
        }

        await _handler.HandleAsync(next, context, policy, authorizeResult);
    }
}