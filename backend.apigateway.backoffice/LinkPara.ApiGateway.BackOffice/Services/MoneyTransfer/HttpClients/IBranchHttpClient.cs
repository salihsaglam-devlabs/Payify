using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Branch;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IBranchHttpClient
{
    Task<PaginatedList<BranchDto>> GetListAsync(GetBranchesRequest request);
    Task SaveAsync(SaveBranchRequest request);
    Task PatchAsync(Guid id, JsonPatchDocument<UpdateBranchRequest> request);
}