using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMccHttpClient
{
    Task<MccDto> GetByIdAsync(Guid id);
    Task<MccDto> GetByCodeAsync(string code);
    Task<PaginatedList<MccDto>> GetAllAsync(GetFilterMccRequest request);
    Task SaveAsync(SaveMccRequest request);
    Task UpdateAsync(UpdateMccRequest request);
    Task DeleteMccAsync(Guid id);
    Task<UpdateMccRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateMccRequest> mccPatch);
}
