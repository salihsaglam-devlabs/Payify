using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IAcquireBankHttpClient
{
    Task<PaginatedList<AcquireBankDto>> GetAllAsync(GetFilterAcquireBankRequest request);
    Task<AcquireBankDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveAcquireBankRequest request);
    Task UpdateAsync(UpdateAcquireBankRequest request);
    Task DeleteAcquireBankAsync(Guid id);
    Task<UpdateAcquireBankRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateAcquireBankRequest> acquireBank);
}
