using LinkPara.Cache;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Requests;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Pagination;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices;

public class PaycoreClientService
{
    private PaycoreTokenResponse cachedToken;
    private readonly PaycoreVaultSettings settings;
    private readonly ICacheService _cacheService;
    private readonly IVaultClient _vaultClient;
    private HttpClient _client;
    private readonly IConfiguration _configuration;
    private readonly PaycoreSettings _paycoreSettings;

    public PaycoreClientService(ICacheService cacheService, IVaultClient vaultClient, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _vaultClient = vaultClient;
        _configuration = configuration;
        _paycoreSettings = new PaycoreSettings();
        settings = _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
    }

    private async Task<string> GetPaycoreTokenAsync()
    {
        cachedToken = _cacheService.Get<PaycoreTokenResponse>("PaycoreToken");

        if (cachedToken is null || DateTime.Now > cachedToken.ExpireDate)
        {
            var token = await GetToken();
            token.ExpireDate = DateTime.Now.AddSeconds(settings.SessionTimeout);
            _cacheService.Add("PaycoreToken", token, 60);
            cachedToken = token;
        }
        return cachedToken.result.token;
    }

    private async Task<PaycoreTokenResponse> GetToken()
    {
        _client = new HttpClient();

        _client.DefaultRequestHeaders.Add("x-api-version", _paycoreSettings.ApiVersion);
        _client.DefaultRequestHeaders.Add("x-channel", _paycoreSettings.Channel);
        _client.DefaultRequestHeaders.Add("x-language", _paycoreSettings.Language);

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        var tokenRequest = new PaycoreTokenRequest
        {
            mbrId = settings.MbrId,
            password = settings.Password,
            sessionTimeout = settings.SessionTimeout,
            userCode = settings.UserCode,
        };

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.BaseUrl}{_paycoreSettings.Token}")
        {
            Content = new StringContent(JsonSerializer.Serialize(tokenRequest, options), Encoding.UTF8, "application/json")
        };
        var response = await _client.SendAsync(postRequest);

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaycoreTokenResponse>(responseString);
    }

    public async Task<PaycoreResponseModel<T>> ExecuteAsync<T>(string endPoint, PaycoreRequestType requestType, object parameters = null)
    {
        _client = new HttpClient();

        var token = await GetPaycoreTokenAsync();

        _client.DefaultRequestHeaders.Add("x-token", token);
        _client.DefaultRequestHeaders.Add("x-api-version", _paycoreSettings.ApiVersion);
        _client.DefaultRequestHeaders.Add("x-channel", _paycoreSettings.Channel);
        _client.DefaultRequestHeaders.Add("x-language", _paycoreSettings.Language);

        //todo integration log
        try
        {
            HttpResponseMessage response = new();

            if (requestType == PaycoreRequestType.Get)
            {
                response = await _client.GetAsync(endPoint);
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                };

                var request = new HttpRequestMessage(requestType == PaycoreRequestType.Post ? HttpMethod.Post : HttpMethod.Put, endPoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(parameters, options), Encoding.UTF8, "application/json")
                };

                response = await _client.SendAsync(request);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaycoreResponseModel<T>>(responseString);

            return result;
        }
        catch (Exception exception)
        {
            //todo elastic
            return new PaycoreResponseModel<T>
            {
                exception = new PaycoreException(),
                message = exception.Message
            };
        }
    }

    public async Task<T> ExecuteGenericAsync<T>(string endPoint, PaycoreRequestType requestType, object parameters = null)
    {
        _client = new HttpClient();

        var token = await GetPaycoreTokenAsync();

        _client.DefaultRequestHeaders.Add("x-token", token);
        _client.DefaultRequestHeaders.Add("x-api-version", _paycoreSettings.ApiVersion);
        _client.DefaultRequestHeaders.Add("x-channel", _paycoreSettings.Channel);
        _client.DefaultRequestHeaders.Add("x-language", _paycoreSettings.Language);

        //todo integration log
        try
        {
            HttpResponseMessage response = new();

            if (requestType == PaycoreRequestType.Get)
            {
                response = await _client.GetAsync(endPoint);
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                };

                var request = new HttpRequestMessage(requestType == PaycoreRequestType.Post ? HttpMethod.Post : HttpMethod.Put, endPoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(parameters, options), Encoding.UTF8, "application/json")
                };

                response = await _client.SendAsync(request);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseString);

            return result;
        }
        catch (Exception exception)
        {
            //todo elastic
            throw exception;
        }
    }

    public string CreateUrlWithParams<T>(string baseUrl, T request, bool fillWithAllParameters = false)
    {
        baseUrl += '?';

        var type = fillWithAllParameters ? request.GetType() : typeof(SearchQueryParams);

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(request);

            if (value is not null)
            {
                if (value.GetType() == typeof(DateTimeOffset))
                {
                    value = $"{(DateTimeOffset)value:yyyy-MM-ddTHH:mm:ss.fffZ}";
                }
                else if (value.GetType() == typeof(DateTime))
                {
                    baseUrl += $"{property.Name}={HttpUtility.UrlEncode($"{(DateTime)value:yyyy-MM-ddTHH:mm:ss}")}&";
                }

                baseUrl += $"{property.Name}={value}&";
            }
        }

        return baseUrl[..^1];
    }
}
