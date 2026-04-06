using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ITransactionHttpClient
{
    Task<TransactionDto> GetTransactionDetailsAsync(Guid id);
    Task<PaginatedList<TransactionDto>> GetWalletTransactionsAsync(GetWalletTransactionsRequest request);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(TransactionSummaryRequest request);
    Task<PaginatedList<TransactionDto>> GetCustodyWalletTransactionsAsync(GetCustodyWalletTransactionsRequest request);
    Task<FastTransactionAmountsDto> GetUserFastTransactionAmountsAsync(Guid walletId);
}
