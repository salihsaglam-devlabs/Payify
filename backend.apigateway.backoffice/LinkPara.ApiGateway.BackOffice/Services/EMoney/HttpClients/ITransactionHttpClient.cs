using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
public interface ITransactionHttpClient
{
    Task<TransactionAdminDto> GetAdminTransactionAsync(Guid id);
    Task<PaginatedList<TransactionAdminDto>> GetAdminTransactionsAsync(GetTransactionsRequest request);
}