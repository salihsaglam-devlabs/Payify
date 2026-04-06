using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IMoneyTransferTransactionsHttpClient
{
    Task<MoneyTransferTransactionsDto> GetTransactionByIdAsync(Guid id);
    Task<PaginatedList<MoneyTransferTransactionsDto>> GetTransactionsAsync(GetMoneyTransferTransactionsRequest request);
    Task CancelAsync(Guid id);
    Task ManualPaymentAsync(Guid id);
}
