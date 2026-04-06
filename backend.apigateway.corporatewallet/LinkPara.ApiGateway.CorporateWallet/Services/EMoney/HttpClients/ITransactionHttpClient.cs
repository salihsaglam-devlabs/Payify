using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ITransactionHttpClient
{
    Task<TransactionDto> GetTransactionDetailsAsync(Guid id);
    Task<PaginatedList<TransactionDto>> GetWalletTransactionsAsync(GetWalletTransactionsRequest request);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(TransactionSummaryRequest request);
}
