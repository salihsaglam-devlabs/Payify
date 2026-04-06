using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
public interface IScSoftHttpClient
{
    Task<ScSoftAtapiResponse> CheckIdentityIsNewAsync(CheckIdentityIsNewRequest request);
    Task<ScSoftAtapiResponse> CheckKpsInformationsAsync(CheckKpsInformationsRequest request);
    Task<ScSoftAtapiResponse> CheckFrontIdentityAsync(CheckFrontIdentityRequest request);
    Task<ScSoftAtapiResponse> CheckRearIdentityAsync(CheckRearIdentityRequest request);
    Task<ScSoftAtapiResponse> CheckNfcAsync(CheckNfcRequest request);
    Task<ScSoftAtapiResponse> CheckHeadPoseAsync(CheckHeadPoseRequest request);
    Task<ScSoftAtapiResponse> CheckSpoofAsync(CheckSpoofRequest request);
    Task<ScSoftAtapiResponse> CheckSimilarityRateAsync(CheckSimilarityRateRequest request);
}
