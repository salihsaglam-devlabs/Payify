using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Authorization.Scopes;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public HasScopeRequirement(string scope)
    {
        Scope = scope;
    }
}