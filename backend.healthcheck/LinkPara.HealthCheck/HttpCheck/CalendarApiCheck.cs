using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.HttpCheck;

public class CalendarApiCheck : CheckBase, IHealthCheck
{
    public CalendarApiCheck(string baseUrl) : base(baseUrl) { }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync($"/v1/Holidays?CountryCode=TUR", cancellationToken);
    }
}
