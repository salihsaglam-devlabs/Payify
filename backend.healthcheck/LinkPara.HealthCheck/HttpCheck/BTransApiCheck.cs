using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.HttpCheck;

public class BTransApiCheck : CheckBase, IHealthCheck
{
    public BTransApiCheck(string baseUrl) : base(baseUrl)
    {

    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync($"/HealthCheck", cancellationToken);
    }
}