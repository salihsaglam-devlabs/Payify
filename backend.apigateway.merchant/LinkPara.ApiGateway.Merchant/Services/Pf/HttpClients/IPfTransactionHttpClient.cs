using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IPfTransactionHttpClient
{
    Task<PaginatedList<PfTransactionDto>> GetAllAsync(GetAllTransactionRequest request);
}
