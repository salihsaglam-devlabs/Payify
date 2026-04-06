using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.OpenBanking.Authorization.Scopes;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public HasScopeRequirement(string scope)
    {
        Scope = scope;
    }
}