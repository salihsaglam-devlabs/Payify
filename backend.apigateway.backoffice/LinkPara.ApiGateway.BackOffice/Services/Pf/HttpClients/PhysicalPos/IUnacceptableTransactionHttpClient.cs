using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public interface IUnacceptableTransactionHttpClient
{
    Task RetryUnacceptableAsync(RetryUnacceptableTransactionRequest request);
    
    Task<PaginatedList<PhysicalPosUnacceptableTransactionDto>> GetAllUnacceptableTransactionsAsync(
        GetAllUnacceptableTransactionRequest request);

    Task<UnacceptableTransactionDetailResponse> GetDetailsByIdAsync(Guid id);

    Task<PhysicalPosUnacceptableTransactionDto> UpdateStatusAsync(Guid id,
        JsonPatchDocument<UpdateUnacceptableTransactionRequest> unacceptableTransaction);
}