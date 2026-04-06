using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Representative;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IRepresentativeHttpClient
{
    Task<PaginatedList<RepresentativeDto>> GetListAsync(GetRepresentativesRequest request);
    Task SaveAsync(SaveRepresentativeRequest request);
    Task PatchAsync(Guid id, JsonPatchDocument<UpdateRepresentativeRequest> request);
}