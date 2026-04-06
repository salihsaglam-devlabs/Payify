using Microsoft.AspNetCore.Authorization;


namespace LinkPara.Authentication.Authorization.Scopes;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public HasScopeRequirement(string scope)
    {
        Scope = scope;
    }
}

