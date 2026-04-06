using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IPostingBalanceHttpClient
{
    Task<ActionResult<PostingBalanceResponse>> GetAllAsync(GetAllPostingBalanceRequest request);
    Task<ActionResult<PostingBalanceDto>> GetByIdAsync(Guid id);
    Task<List<PostingBalanceStatusModel>> GetStatusCountAsync(PostingBalanceStatusRequest request);
    Task PatchAsync(Guid id, JsonPatchDocument<PatchPostingBalanceRequest> request);
}
