using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.Merchant.Authorization.Scopes;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public HasScopeRequirement(string scope)
    {
        Scope = scope;
    }
}