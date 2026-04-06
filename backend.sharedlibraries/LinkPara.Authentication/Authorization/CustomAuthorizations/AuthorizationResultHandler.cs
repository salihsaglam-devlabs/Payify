using LinkPara.Authentication.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LinkPara.Authentication.Authorization.CustomAuthorizations;

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
            context.Response.Headers["Content-Type"] = "application/problem+json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(details));
            await context.Response.CompleteAsync();
        }

        await _handler.HandleAsync(next, context, policy, authorizeResult);
    }
}
