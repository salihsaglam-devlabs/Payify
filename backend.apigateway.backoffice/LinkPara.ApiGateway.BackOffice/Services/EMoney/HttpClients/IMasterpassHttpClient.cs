using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IMasterpassHttpClient
{
    Task<TopupCancelResponse> TopupCancelAsync(MasterpassCancelRequest request);
}