using LinkPara.ApiGateway.CorporateWallet.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Authorization.CustomAuthorizations;

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

        await _handler.HandleAsync(next, context, policy, authorizeResult);
    }
}