using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LinkPara.HealthCheck.Models;

public static class MapOptions
{
    public static HealthCheckOptions GetOptions(Tags tag)
    {
        return new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            Predicate = (check) => check.Tags.Contains(tag.ToString())
        };
    }
}
