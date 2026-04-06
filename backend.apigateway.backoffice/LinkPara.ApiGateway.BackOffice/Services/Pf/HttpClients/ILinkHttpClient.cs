using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface ILinkHttpClient
{
    Task<PaginatedList<LinkDto>> GetAllAsync(GetFilterLinkRequest request);
    Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync(GetPaymentDetailRequest request);
}
