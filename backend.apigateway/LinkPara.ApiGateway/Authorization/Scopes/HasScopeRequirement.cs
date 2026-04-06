using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.Authorization.Scopes;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public HasScopeRequirement(string scope)
    {
        Scope = scope;
    }
}
