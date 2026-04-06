using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantHttpClient : HttpClientBase, IMerchantHttpClient
{
    public MerchantHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task ApproveAsync(ApproveMerchantRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Merchants/approve", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteMerchantAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/Merchants/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<MerchantDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Merchants/{id}");
        var merchant = await response.Content.ReadFromJsonAsync<MerchantDto>();
        return merchant ?? throw new InvalidOperationException();
    }

    public async Task<MerchantMaskedDto> GetByIdMaskedAsync(Guid id)
    {
        var response = await GetAsync($"v1/Merchants/{id}");
        var merchant = await response.Content.ReadFromJsonAsync<MerchantDto>();

        var newResponse = new MerchantMaskedDto
        {
            Id = merchant.Id,
            Name = merchant.Name,
            Number = merchant.Number,
            AuthorizedPersonName = merchant.Customer.AuthorizedPerson.Name,
            AuthorizedPersonSurname = merchant.Customer.AuthorizedPerson.Surname,
            AuthorizedPersonMaskedPhoneNumber = merchant.Customer.AuthorizedPerson.MobilePhoneNumber,
        };

        if (!CanSeeSensitiveData())
        {
            newResponse.AuthorizedPersonSurname = SensitiveDataHelper.MaskSensitiveData("Name", merchant.Customer.AuthorizedPerson.Surname);
            newResponse.AuthorizedPersonMaskedPhoneNumber = SensitiveDataHelper.MaskSensitiveData("CallCenterPhoneNumber", merchant.Customer.AuthorizedPerson.MobilePhoneNumber);
        }
        return newResponse ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantRequest request)
    {
        var url = CreateUrlWithParams($"v1/Merchants", request, true);
        var response = await GetAsync(url);
        var merchants = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDto>>();
        return merchants ?? throw new InvalidOperationException();
    }

    public async Task<PatchMerchantRequest> PatchAsync(Guid id, JsonPatchDocument<PatchMerchantRequest> merchantPatch)
    {
        var response = await PatchAsync($"v1/Merchants/update/{id}", merchantPatch);
        var merchant = await response.Content.ReadFromJsonAsync<PatchMerchantRequest>();
        return merchant ?? throw new InvalidOperationException();
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

    public async Task<IKSResponse<AnnulmentResponse>> SaveAnnulmentAsync(Guid merchantId, SaveAnnulmentRequest saveAnnulmentRequest)
    {
        var response = await PostAsJsonAsync($"v1/Merchants/{merchantId}/annulments", saveAnnulmentRequest);
        var result = await response.Content.ReadFromJsonAsync<IKSResponse<AnnulmentResponse>>();
        return result ?? throw new InvalidOperationException();
    }

    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(MerchantPricingPreviewRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Merchants/pricing-preview", request);
        var pricingProfilePreview = await response.Content.ReadFromJsonAsync<PricingProfilePreviewResponse>();
        return pricingProfilePreview ?? throw new InvalidOperationException();
    }

    public async Task<string> GetAuthorizedPersonPhoneNumberAsync(Guid id)
    {
        var response = await GetAsync($"v1/Merchants/authorizedPersonPhoneNumber/{id}");
        return await response.Content.ReadAsStringAsync();
    }
}