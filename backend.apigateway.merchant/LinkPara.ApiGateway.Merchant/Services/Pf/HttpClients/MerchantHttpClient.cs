using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using ValidateIbanRequest = LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests.ValidateIbanRequest;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class MerchantHttpClient : HttpClientBase, IMerchantHttpClient
{
    public MerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MerchantSummaryDto> GetSummaryByIdAsync()
    {
        var response = await GetAsync($"v1/Merchants/summary");
        var merchant = await response.Content.ReadFromJsonAsync<MerchantSummaryDto>();
        return merchant ?? throw new InvalidOperationException();
    }

    public async Task PutAsync(Guid id, UpdateMerchantPanelDto request)
    {
        var response = await PutAsJsonAsync($"v1/Merchants/update/panel/{id}", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<MerchantResponse> UpdateAsync(UpdateMerchantRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Merchants", request);
        var merchantResponse = await response.Content.ReadFromJsonAsync<MerchantResponse>();
        return merchantResponse ?? throw new InvalidOperationException();
    }

    public async Task<MerchantApiKeyDto> GetApiKeysAsync(string publicKey)
    {
        var publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(publicKey);
        var publicKeyEncoded = Convert.ToBase64String(publicKeyBytes);

        var response = await GetAsync($"v1/Merchants/apiKeys?PublicKeyEncoded={publicKeyEncoded}");
        var responseString = await response.Content.ReadAsStringAsync();
        var apiKeys = JsonSerializer.Deserialize<MerchantApiKeyDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return apiKeys;
    }

    public async Task<MerchantApiKeyPatch> ApiKeyPatchAsync(Guid merchantId, JsonPatchDocument<MerchantApiKeyPatch> merchantApiKeyPatch)
    {
        var response = await PatchAsync($"v1/Merchants/{merchantId}/apiKeys", merchantApiKeyPatch);
        var merchantApiKey = await response.Content.ReadFromJsonAsync<MerchantApiKeyPatch>();
        return merchantApiKey ?? throw new InvalidOperationException();
    }

    public async Task<MerchantApiKeyDto> GenerateApiKeysAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/Merchants/{merchantId}/generate-apiKeys");
        var merchantApiKeys = await response.Content.ReadFromJsonAsync<MerchantApiKeyDto>();
        return merchantApiKeys ?? throw new InvalidOperationException();
    }

    public async Task<ValidateIbanResponse> ValidateIbanAsync(ValidateIbanRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Merchants/validate-iban", request);
        var result = await response.Content.ReadFromJsonAsync<ValidateIbanResponse>();
        return result ?? throw new InvalidOperationException();
    }
}