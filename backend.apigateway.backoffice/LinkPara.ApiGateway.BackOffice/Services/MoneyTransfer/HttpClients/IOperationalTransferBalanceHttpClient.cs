using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IOperationalTransferBalanceHttpClient
{
    Task<OperationalTransferBalanceDto> GetOperationalTransferBalanceByIdAsync(Guid id);
    Task<PaginatedList<OperationalTransferBalanceDto>> GetOperationalTransferBalanceListAsync(GetOperationalTransferBalanceListRequest request);
    Task PatchOperationalTransferBalanceAsync(Guid id, JsonPatchDocument<PatchOperationalTransferBalanceRequest> request);
    Task SaveOperationalTransferBalanceAsync(SaveOperationalTransferBalanceRequest request);
}
