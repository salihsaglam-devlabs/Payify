namespace LinkPara.ApiGateway.CorporateWallet.Authorization.Scopes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class HasScopeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
{
    public HasScopeAttribute(string scope) : base(scope) { }
}