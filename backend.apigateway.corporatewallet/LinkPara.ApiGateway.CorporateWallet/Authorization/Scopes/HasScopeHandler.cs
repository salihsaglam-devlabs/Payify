using LinkPara.SharedModels.Authorization.Models;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.CorporateWallet.Authorization.Scopes;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        if (context.User.FindFirst(c =>
            (c.Type == ClaimKey.UserScope || c.Type == ClaimKey.RoleScope) &&
            c.Value.ToUpperInvariant() == requirement.Scope.ToUpperInvariant()) is not null)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
