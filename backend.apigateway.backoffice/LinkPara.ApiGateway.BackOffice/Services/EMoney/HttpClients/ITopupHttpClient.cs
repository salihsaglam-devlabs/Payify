using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ITopupHttpClient
{
    Task<TopupCancelResponse> TopupCancelAsync(TopupCancelRequest request);
    Task<PaginatedList<TopupResponse>> GetListAsync(GetTopupListRequest request);
    Task TopupReturnToWalletAsync(TopupReturnToWalletRequest request);
    Task TopupUpdateStatusAsync(TopupUpdateStatusRequest request);
}
