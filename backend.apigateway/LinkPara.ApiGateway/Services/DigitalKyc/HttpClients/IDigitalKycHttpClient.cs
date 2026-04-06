using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients
{
    public interface IDigitalKycHttpClient
    {
        Task<DigitalKycStartResponse> DigitalKycStartAsync(DigitalKycStartRequest request);
        Task<IntegrationGetResponse> IntegrationGetAsync(IntegrationGetRequest request);
        Task DigitalKycEndAsync(DigitalKycEndRequest request);
        Task<bool> GetKycStateByUserId(string userId);
    }
}