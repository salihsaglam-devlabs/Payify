using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Headers;

namespace LinkPara.HealthCheck.HttpCheck;

public class CheckBase
{
    public HttpClient _client;

    public CheckBase(string baseUrl)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<HealthCheckResult> SendRequestAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            if (_client.BaseAddress.ToString().Contains("http://unusedservice.com"))
            {
                return await Task.FromResult(HealthCheckResult.Degraded());
            } 
            using var response = await _client.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await Task.FromResult(HealthCheckResult.Healthy($"{(int)response.StatusCode} - {response.StatusCode}"));
            }

            else
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy($"{(int)response.StatusCode} - {response.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy(ex.Message));
        }
    }
}
