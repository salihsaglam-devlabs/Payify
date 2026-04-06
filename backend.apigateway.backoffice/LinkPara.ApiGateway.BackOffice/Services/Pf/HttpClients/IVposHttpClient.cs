using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IVposHttpClient
{
    Task<VposDto> GetByIdAsync(Guid id);
    Task<MerchantVposDto> GetByReferenceNumberAsync(string bkmReferenceNumber);
    Task<PaginatedList<VposDto>> GetFilterListAsync(GetFilterVposRequest request);
    Task SaveAsync(SaveVposRequest request);
    Task UpdateAsync(UpdateVposRequest request);
    Task DeleteVposAsync(Guid id);
    Task<PatchVposRequest> PatchAsync(Guid id, JsonPatchDocument<PatchVposRequest> vposPatch);
}
