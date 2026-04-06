using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault.Models;
using Microsoft.Extensions.Logging;

namespace LinkPara.HttpProviders.Vault;

public class VaultClient : IVaultClient
{
    private readonly HttpClient _client;
    private readonly ICacheService _cacheService;
    private readonly string _roleId;
    private readonly string _secretId;

    private DateTime _tokenExpireDate;
    private const string TokenHeaderName = "X-Vault-Token";

    public VaultClient(HttpClient client, string roleId, string secretId,
        ICacheService cacheService)
    {
        _client = client;
        _roleId = roleId;
        _secretId = secretId;
        _cacheService = cacheService;

        _tokenExpireDate = DateTime.MinValue;
    }

    #region PrivateMethods
    private volatile bool _refreshing = false;
    private readonly object _lockObject = new object();

    private void RefreshToken()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (!_refreshing)
        {
            lock (_lockObject)
            {
                if (!_refreshing)
                {
                    _refreshing = true;
                    try
                    {
                        if (environment?.ToLowerInvariant() != "production" || DateTime.Now > _tokenExpireDate)
                        {
                            LoginClientAsync().GetAwaiter().GetResult();
                        }
                    }
                    finally
                    {
                        _refreshing = false;
                    }
                }
            }
        }
    }

    private async Task LoginClientAsync()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment?.ToLowerInvariant() == "staging")
        {
            // Required Headers for Enterprise Vault Integration:
            if (!_client.DefaultRequestHeaders.Contains("X-Vault-Namespace"))
            {
                _client.DefaultRequestHeaders.Add("X-Vault-Namespace", "admin");
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/auth/approle/login")
        {
            Content = JsonContent.Create(new VaultLoginRequest
            {
                role_id = _roleId,
                secret_id = _secretId
            })
        };

        var response = await _client.PostAsJsonAsync<VaultLoginRequest>($"/v1/auth/approle/login", new()
        {
            role_id = _roleId,
            secret_id = _secretId
        });


        if (!response.IsSuccessStatusCode)
        {
            var failedContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login failed! {failedContent}");
        }
        var vaultLoginResponse = await response.Content.ReadFromJsonAsync<VaultLoginResponse>();

        _tokenExpireDate = DateTime.Now.AddSeconds(vaultLoginResponse.auth.lease_duration);

        while (_client.DefaultRequestHeaders.TryGetValues(TokenHeaderName, out var token))
        {
            _client.DefaultRequestHeaders.Remove(TokenHeaderName);
        }

        _client.DefaultRequestHeaders.Add(TokenHeaderName, vaultLoginResponse.auth.client_token);
    }
    #endregion

    public async Task<T> GetSecretValueAsync<T>(string engine, string path, string key = null)
    {
        var cacheKey = GetCacheKey(engine, path, key);

        if (key is not null && await _cacheService.ContainsKeyAsync<T>(cacheKey))
        {
            return _cacheService.Get<T>(cacheKey);
        }

        RefreshToken();

        VaultSecretResponse response;

        response = await _client.GetFromJsonAsync<VaultSecretResponse>($"/v1/{engine}/data/{path}");

        var responseData = response?.data?.data;

        if (responseData?.ValueKind == JsonValueKind.Object && responseData?.ToString() == "{}")
        {
            return default(T);
        }

        if (key is not null)
        {
            var cacheValue = response.data.data.GetProperty(key).Deserialize<T>();
            _cacheService.Add(cacheKey, cacheValue, 60);

            return cacheValue;
        }

        return response.data.data.Deserialize<T>();
    }
    public async Task PostSecretValueAsync<T>(string engine, string path, string kvData = null)
    {

        var content = new StringContent(kvData, Encoding.UTF8, "application/json");
        var result = await _client.PostAsync($"/v1/{engine}/data/{path}", content);

        if (!result.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    private static string GetCacheKey(string engine, string path, string key)
    {
        return new StringBuilder(engine)
            .Append('_').Append(path)
            .Append('_').Append(key)
            .ToString();
    }

    public T GetSecretValue<T>(string engine, string path, string key = null)
    {
        return GetSecretValueAsync<T>(engine, path, key).GetAwaiter().GetResult();
    }
}
