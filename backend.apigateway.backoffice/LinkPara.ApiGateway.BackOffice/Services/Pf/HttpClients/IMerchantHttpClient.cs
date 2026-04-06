using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantHttpClient
{
    Task<MerchantDto> GetByIdAsync(Guid id);
    Task<string> GetAuthorizedPersonPhoneNumberAsync(Guid id);
    Task<MerchantMaskedDto> GetByIdMaskedAsync(Guid id);
    Task<MerchantApiKeyDto> GenerateApiKeysAsync(Guid merchantId);
    Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantRequest request);
    Task<MerchantResponse> UpdateAsync(UpdateMerchantRequest request);
    Task DeleteMerchantAsync(Guid id);
    Task ApproveAsync(ApproveMerchantRequest request);
    Task<PatchMerchantRequest> PatchAsync(Guid id, JsonPatchDocument<PatchMerchantRequest> merchantPatch);
    Task<MerchantApiKeyPatch> ApiKeyPatchAsync(Guid merchantId, JsonPatchDocument<MerchantApiKeyPatch> merchantApiKeyPatch);
    Task<MerchantApiKeyDto> GetApiKeysAsync(string publicKey);
    Task<IKSResponse<AnnulmentResponse>> SaveAnnulmentAsync(Guid merchantId, SaveAnnulmentRequest saveAnnulmentRequest);
    Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(MerchantPricingPreviewRequest request);
}
