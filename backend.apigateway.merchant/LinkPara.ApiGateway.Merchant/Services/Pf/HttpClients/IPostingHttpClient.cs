using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IPostingHttpClient
{
    Task<PaginatedList<PostingBillDto>> GetBillsAsync(Guid merchantId, GetPostingBillRequest request);
}