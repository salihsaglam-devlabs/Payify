using System.Net;
using System.Text.Json;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinkPara.ApiGateway.BackOffice.Filters.LoginActionFilter;

public class LoginActionFilter : IAsyncActionFilter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginActionFilter> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly ICacheService _cacheService;

    private const string LicenceUrl = "https://license.linktera.xyz/api/verify";

    private static readonly List<string> Tenants = new List<string>
    {
        "parakolay",
        "binpay",
        "payify"
    };

    public LoginActionFilter(IHttpClientFactory httpClientFactory, ILogger<LoginActionFilter> logger, IVaultClient vaultClient, ICacheService cacheService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _vaultClient = vaultClient;
        _cacheService = cacheService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var tenant = Environment.GetEnvironmentVariable("Tenant")?.ToLower();
            
            if (!Tenants.Contains(tenant) || environment?.ToLowerInvariant() == "development")
            {
                await next();
                return;
            }
            
            var endOfDay = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            var timeRemaining = endOfDay - DateTime.Now;
            var minutesRemaining = (int)Math.Ceiling(timeRemaining.TotalMinutes);
            
            var licenceCheckResponse = _cacheService.Get<LicenceCheckResponse>("PayifyLicence");

            if (licenceCheckResponse is null || licenceCheckResponse.status != 200 || licenceCheckResponse.lastExecution < DateTime.Today)
            {
                licenceCheckResponse = await GetPayifyLicenceResponseAsync();
                _cacheService.Add("PayifyLicence", licenceCheckResponse, minutesRemaining);
            }

            if (licenceCheckResponse.status != 200 && licenceCheckResponse.status != 500)
            {
                var problemDetails = new ProblemDetails
                {
                    Detail = "Payify licence is not active!",
                };
                problemDetails.Extensions.Add("code", (int)HttpStatusCode.Forbidden);

                context.Result = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Content = JsonSerializer.Serialize(problemDetails),
                    ContentType = "application/json"
                };
                return;
            }

            await next();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error occured on payify licence check: {exception}");
            await next();
        }
    }

    private async Task<LicenceCheckResponse> GetPayifyLicenceResponseAsync()
    {
        string licenceKey;
        string licenceSecret;
        try
        {
            
            licenceKey = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "PayifyLicence", "Key");
            licenceSecret = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "PayifyLicence", "Secret");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Cannot fetch Payify licence keys from vault: {exception}");
            return new LicenceCheckResponse
            {
                status = 400,
                message = "Payify licence is not active!",
                remaining = 0,
                lastExecution = DateTime.Now
            };
        }
        
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(LicenceUrl, new LicenceCheckRequest
        {
            key = licenceKey,
            secret = licenceSecret
        });
        
        if (!response.IsSuccessStatusCode)
        {
            if((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
            {
                _logger.LogError($"Payify licence control service returns {(int)response.StatusCode}. Skipping licence check");
                return new LicenceCheckResponse
                {
                    status = 500,
                    message = "Payify licence control service is not active!",
                    remaining = 0,
                    lastExecution = DateTime.Now
                };
            }
            _logger.LogError($"Payify licence control returns not successful");
            return new LicenceCheckResponse
            {
                status = 400,
                message = "Payify licence is not active!",
                remaining = 0,
                lastExecution = DateTime.Now
            };
        }
        
        var licence = await response.Content.ReadFromJsonAsync<LicenceCheckResponse>();
        licence.lastExecution = DateTime.Now;
        
        if (licence.remaining < 1)
        {
            _logger.LogError($"Payify licence control returns remaining day as {licence.remaining}");
            return new LicenceCheckResponse
            {
                status = 400,
                message = "Payify licence is not active!",
                remaining = 0,
                lastExecution = DateTime.Now
            };
        }

        return licence;
    }
}