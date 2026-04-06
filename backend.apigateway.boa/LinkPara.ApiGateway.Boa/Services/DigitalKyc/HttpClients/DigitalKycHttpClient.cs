using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Services.DigitalKyc.Models.Requests;
using System.Text;
using System.Text.Json;

namespace LinkPara.ApiGateway.Boa.Services.DigitalKyc.HttpClients;

public class DigitalKycHttpClient : HttpClientBase, IDigitalKycHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public DigitalKycHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter) : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task KycUpdateAsync(KycUpdateRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<KycUpdateRequest, KycUpdateServiceRequest>(request);

        var response = await PostAsJsonAsync($"v1/DigitalKyc/update-kyc", clientRequest);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }
}
