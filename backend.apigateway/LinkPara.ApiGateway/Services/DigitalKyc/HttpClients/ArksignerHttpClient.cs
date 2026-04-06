using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
public class ArksignerHttpClient : HttpClientBase, IArksignerHttpClient
{
    public ArksignerHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<ArksignerServiceResponse> CheckFaceMatchAsync(CheckFaceMatchRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Arksigner/check-face-match", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<ArksignerServiceResponse>();
    }

    public async Task<ArksignerServiceResponse> CheckIdentityCardInformationsAsync(CheckIdentityCardInformationsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Arksigner/check-id", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<ArksignerServiceResponse>();
    }

    public async Task<ArksignerServiceResponse> CheckNfcInformationsAsync(CheckNfcInformationsRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Arksigner/check-nfc", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<ArksignerServiceResponse>();
    }

    public async Task<ArksignerServiceResponse> CompleteKycProcessAsync(CompleteKycProcessRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Arksigner/complete-kyc", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<ArksignerServiceResponse>();
    }

    public async Task<ArksignerServiceResponse> StartKycProcessAsync(StartKycProcessRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Arksigner/start-kyc", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        return await response.Content.ReadFromJsonAsync<ArksignerServiceResponse>();
    }
}
