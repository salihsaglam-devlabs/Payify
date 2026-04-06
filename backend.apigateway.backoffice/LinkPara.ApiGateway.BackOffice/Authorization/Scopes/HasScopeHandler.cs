using LinkPara.SharedModels.Authorization.Models;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Authorization.Scopes;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        if (context.User.HasClaim(c =>
            (c.Type == ClaimKey.UserScope || c.Type == ClaimKey.RoleScope) &&
            c.Value.ToUpperInvariant() == requirement.Scope.ToUpperInvariant()))
        {
            var roleTypes = context.User.FindAll(c => c.Type == "RoleType")?.Select(x => x.Value).ToList();

            if (roleTypes != null && (roleTypes.Contains("Internal") || roleTypes.Contains("Representative") || roleTypes.Contains("Branch")))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}