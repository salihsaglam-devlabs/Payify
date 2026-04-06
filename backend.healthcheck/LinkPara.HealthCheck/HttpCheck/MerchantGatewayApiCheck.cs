using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.HttpCheck;

public class MerchantGatewayApiCheck : CheckBase, IHealthCheck
{
    public MerchantGatewayApiCheck(string baseUrl) : base(baseUrl)
    {
    }


    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync($"/", cancellationToken);
    }
}
