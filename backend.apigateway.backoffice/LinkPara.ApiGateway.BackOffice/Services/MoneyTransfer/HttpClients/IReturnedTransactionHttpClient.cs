using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients
{
    public interface IReturnedTransactionHttpClient
    {
        Task<ReturnedTransactionDto> GetReturnedTransactionByIdAsync(Guid id);
        Task<PaginatedList<ReturnedTransactionDto>> GetReturnedTransactionsAsync(GetReturnedTransactionsRequest request);
        Task CancelReturnedTransactionAsync(Guid id);
    }
}
