using LinkPara.Emoney.ApiGateway.Models.Requests;
using LinkPara.Emoney.ApiGateway.Models.Responses;

namespace LinkPara.Emoney.ApiGateway.Services.HttpClients;

public interface IProvisionHttpClient
{
    Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionRequest request);
    Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionRequest request);
    Task<ProvisionResponse> ProvisionAsync(ProvisionRequest request);
}
