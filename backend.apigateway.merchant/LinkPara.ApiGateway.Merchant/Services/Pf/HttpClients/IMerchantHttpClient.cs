using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.HttpProviders.KKB.Models;
using Microsoft.AspNetCore.JsonPatch;
using ValidateIbanRequest = LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests.ValidateIbanRequest;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantHttpClient
{
    Task<MerchantSummaryDto> GetSummaryByIdAsync();
    Task<MerchantApiKeyDto> GenerateApiKeysAsync(Guid merchantId);
    Task<MerchantResponse> UpdateAsync(UpdateMerchantRequest request);
    Task PutAsync(Guid id, UpdateMerchantPanelDto request);
    Task<MerchantApiKeyPatch> ApiKeyPatchAsync(Guid merchantId, JsonPatchDocument<MerchantApiKeyPatch> merchantApiKeyPatch);
    Task<MerchantApiKeyDto> GetApiKeysAsync(string publicKey);
    Task<ValidateIbanResponse> ValidateIbanAsync(ValidateIbanRequest request);
}
