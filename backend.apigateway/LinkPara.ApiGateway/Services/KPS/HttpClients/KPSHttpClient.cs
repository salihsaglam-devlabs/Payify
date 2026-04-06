using LinkPara.ApiGateway.Services.KPS.Models.Request;
using LinkPara.ApiGateway.Services.KPS.Models.Response;

namespace LinkPara.ApiGateway.Services.KPS.HttpClients
{
    public class KPSHttpClient : HttpClientBase, IKPSHttpClient
    {
        public KPSHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }
        public async Task<ValidateIdentityResponse> ValidateIdentityAsync(ValidateIdentityRequest request)
        {
            var response = await PostAsJsonAsync($"v1/KPS/validate-identity", request);
            var result = await response.Content.ReadFromJsonAsync<ValidateIdentityResponse>();
            return result ?? throw new InvalidOperationException();
        }

        public async Task<AddressInformationResponse> GetAddressAsync(AddressInformationRequest request)
        {            
            var url = CreateUrlWithProperties($"v1/KPS/address-information", request);
            var response = await GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<AddressInformationResponse>();
            return result ?? throw new InvalidOperationException();
        }

        public async Task<ValidateCustodyInformationResponse> ValidateCustodyInformationAsync(ValidateCustodyInformationRequest request)
        {
            var response = await PostAsJsonAsync($"v1/KPS/validate-custody-information", request);
            var result = await response.Content.ReadFromJsonAsync<ValidateCustodyInformationResponse>();
            return result ?? throw new InvalidOperationException();
        }
    }
}
