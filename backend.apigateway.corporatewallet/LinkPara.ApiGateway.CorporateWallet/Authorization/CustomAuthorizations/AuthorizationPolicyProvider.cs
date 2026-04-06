using LinkPara.ApiGateway.CorporateWallet.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LinkPara.ApiGateway.CorporateWallet.Authorization.CustomAuthorizations;

public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
        _options = options.Value;
    }

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is null)
        {
            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new HasScopeRequirement(policyName))
                .Build();
            _options.AddPolicy(policyName, policy);
        }
        return policy;
    }
}