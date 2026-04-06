using LinkPara.ApiGateway.BackOffice.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LinkPara.ApiGateway.BackOffice.Authorization.CustomAuthorizations;

public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly ILogger<AuthorizationPolicyProvider> _logger;
    private readonly AuthorizationOptions _options;
    private readonly object _lock = new object();

    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options,
        ILogger<AuthorizationPolicyProvider> logger) : base(options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is null)
        {
            lock (_lock)
            {
                // Double-check to ensure the policy wasn't added by another thread
                policy = _options.GetPolicy(policyName);
                if (policy == null)
                {
                    policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new HasScopeRequirement(policyName))
                        .Build();
                    try
                    {
                        _options.AddPolicy(policyName, policy);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error on adding policy: {e}");
                    }
                }
            }
        }
        return policy;
    }
}