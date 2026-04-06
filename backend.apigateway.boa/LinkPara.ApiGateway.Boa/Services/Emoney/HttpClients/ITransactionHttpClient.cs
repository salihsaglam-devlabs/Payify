using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface ITransactionHttpClient
{
    Task<TransactionDto> GetTransactionDetailsAsync(Guid id);
    Task<PaginatedList<TransactionDto>> GetWalletTransactionsAsync(GetWalletTransactionsRequest request);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(TransactionSummaryRequest request);
    Task<ReceiptDto> GetReceiptAsync(GetReceiptRequest request);
}
