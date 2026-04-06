using LinkPara.ApiGateway.Boa.Services.DigitalKyc.Models.Requests;

namespace LinkPara.ApiGateway.Boa.Services.DigitalKyc.HttpClients;

public interface IDigitalKycHttpClient
{
    Task KycUpdateAsync(KycUpdateRequest request);
}
