namespace LinkPara.ApiGateway.Authorization.Scopes;

public class HasScopeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
{
    public HasScopeAttribute(string scope) : base(scope) { }
}

