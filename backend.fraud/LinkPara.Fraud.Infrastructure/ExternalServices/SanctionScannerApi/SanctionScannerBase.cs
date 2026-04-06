using LinkPara.SharedModels.Exceptions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi;

public abstract class SanctionScannerBase
{
    protected async Task<string> GetAsync(string baseurl, string username, string password)
    {
        using var client = new HttpClient();
        var convertAuth = SanctionScannerHelper.GetBasicAuthorizationKey(username, password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", convertAuth);
        var response = await client.GetAsync(baseurl);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        throw new NotFoundException(baseurl);
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string baseurl, T data, string username, string password)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        var convertAuth = SanctionScannerHelper.GetBasicAuthorizationKey(username, password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", convertAuth);
        var response = await client.PostAsJsonAsync(baseurl, data);

        return response;
    }

    protected string CreateUrlWithParams<T>(string baseUrl, T request)
    {
        baseUrl += '?';

        var type = request.GetType();

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(request);

            if (value is not null)
            {
                if (value.GetType() == typeof(DateTimeOffset))
                {
                    baseUrl += $"{property.Name}={HttpUtility.UrlEncode($"{(DateTimeOffset)value:yyyy-MM-ddTHH:mm:ss.fffZ}")}&";
                }
                else
                {
                    baseUrl += $"{property.Name}={value}&";
                }
            }
        }

        return baseUrl[..^1];
    }
}
