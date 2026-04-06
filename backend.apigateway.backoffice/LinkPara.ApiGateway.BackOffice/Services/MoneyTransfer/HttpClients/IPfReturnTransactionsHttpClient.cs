using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
public interface IPfReturnTransactionsHttpClient
{
    Task<PaginatedList<PfReturnTransactionDto>> GetListAsync(GetPfReturnTransactionsRequest request);
    Task<bool> ReturnPfTransactionAsync(ReturnPfTransactionRequest request);
}
