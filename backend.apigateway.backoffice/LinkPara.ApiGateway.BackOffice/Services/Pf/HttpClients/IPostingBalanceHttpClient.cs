using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IPostingBalanceHttpClient
{
    Task<ActionResult<PostingBalanceResponse>> GetAllAsync(GetAllPostingBalanceRequest request);
    Task<ActionResult<PostingBalanceDto>> GetByIdAsync(Guid id);
    Task<List<PostingBalanceStatusModel>> GetStatusCountAsync(PostingBalanceStatusRequest request);
    Task PatchAsync(Guid id, JsonPatchDocument<PatchPostingBalanceRequest> request);
    Task RetryPostingPaymentAsync(RetryPostingPaymentRequest request);
    Task<ActionResult<PostingBalanceStatisticsResponse>> GetStatisticsAsync(PostingBalanceStatisticsRequest request);
    Task<ActionResult<PaginatedList<PostingPfProfitDto>>> GetAllPostingPfProfits(GetAllPostingPfProfitsRequest request);
}
