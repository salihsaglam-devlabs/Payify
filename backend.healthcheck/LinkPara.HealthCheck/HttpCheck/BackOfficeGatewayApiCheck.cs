using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.HttpCheck;

public class BackOfficeGatewayApiCheck : CheckBase, IHealthCheck
{
    public BackOfficeGatewayApiCheck(string baseUrl) : base(baseUrl)
    {
    }


    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync($"/", cancellationToken);
    }
}
