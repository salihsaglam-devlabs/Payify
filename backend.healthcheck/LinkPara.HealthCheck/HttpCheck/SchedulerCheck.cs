using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.HttpCheck;

public class SchedulerCheck : CheckBase, IHealthCheck
{
    public SchedulerCheck(string baseUrl) : base(baseUrl) { }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync($"/", cancellationToken);
    }
}
