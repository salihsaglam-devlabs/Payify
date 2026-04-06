using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ITierLevelHttpClient
{
    Task<List<TierLevelResponse>> GetTierLevelsAsync(GetTierLevelsRequest request);
    Task<TierLevelResponse> GetTierLevelByIdAsync(Guid id);
    Task CreateTierLevelsAsync(CustomTierLevelDto request);
    Task PatchCustomTierLevelAsync(Guid id, JsonPatchDocument<CustomTierLevelDto> request);
    Task DisableCustomTierLevelAsync(Guid id);
    Task CreateAccountCustomTierAsync(AccountCustomTierDto request);
    Task DeleteAccountCustomTierAsync(Guid id);
}