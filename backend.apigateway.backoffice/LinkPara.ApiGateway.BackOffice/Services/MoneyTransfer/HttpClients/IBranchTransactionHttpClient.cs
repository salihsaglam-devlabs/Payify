using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.BranchTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IBranchTransactionHttpClient
{
    Task<BranchTransactionDto> GetByIdAsync(Guid id);
    Task<PaginatedList<BranchTransactionDto>> GetListAsync(GetBranchTransactionsRequest request);
    Task SaveAsync(SaveBranchTransactionRequest request);
    Task UpdateAsync(UpdateBranchTransactionRequest request);
}