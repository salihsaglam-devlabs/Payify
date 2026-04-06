using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IIncomingTransactionHttpClient
{
    Task<IncomingTransactionDto> GetIncomingTransactionByIdAsync(Guid id);
    Task<PaginatedList<IncomingTransactionDto>> GetIncomingTransactionsAsync(GetIncomingTransactionsRequest request);
    Task ManualPaymentAsync(Guid id);
}
