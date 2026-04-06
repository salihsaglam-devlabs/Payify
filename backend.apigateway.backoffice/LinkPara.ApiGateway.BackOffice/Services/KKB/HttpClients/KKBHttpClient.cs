using LinkPara.HttpProviders.KKB.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.KKB.HttpClients
{
    public class KKBHttpClient : HttpClientBase, IKKBHttpClient
    {
        public KKBHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }
        public async Task<ValidateIbanResponse> ValidateIbanAsync(ValidateIbanRequest request)
        {
            var response = await PostAsJsonAsync($"v1/KKB/validate-iban", request);
            var result = await response.Content.ReadFromJsonAsync<ValidateIbanResponse>();
            return result ?? throw new InvalidOperationException();
        }
    }
}
