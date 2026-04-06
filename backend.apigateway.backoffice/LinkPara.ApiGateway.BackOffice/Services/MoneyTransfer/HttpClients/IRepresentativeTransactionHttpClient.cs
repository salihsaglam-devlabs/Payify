using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.RepresentativeTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IRepresentativeTransactionHttpClient
{
    Task<RepresentativeTransactionDto> GetByIdAsync(Guid id);
    Task<PaginatedList<RepresentativeTransactionDto>> GetListAsync(GetRepresentativeTransactionsRequest request);
    Task SaveAsync(SaveRepresentativeTransactionRequest request);
    Task UpdateAsync(UpdateRepresentativeTransactionRequest request);
}