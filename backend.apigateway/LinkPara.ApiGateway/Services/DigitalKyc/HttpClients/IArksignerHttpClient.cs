using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
public interface IArksignerHttpClient
{
    Task<ArksignerServiceResponse> CheckNfcInformationsAsync(CheckNfcInformationsRequest request);
    Task<ArksignerServiceResponse> CheckFaceMatchAsync(CheckFaceMatchRequest request);
    Task<ArksignerServiceResponse> CheckIdentityCardInformationsAsync(CheckIdentityCardInformationsRequest request);
    Task<ArksignerServiceResponse> StartKycProcessAsync(StartKycProcessRequest request);
    Task<ArksignerServiceResponse> CompleteKycProcessAsync(CompleteKycProcessRequest request);
}
