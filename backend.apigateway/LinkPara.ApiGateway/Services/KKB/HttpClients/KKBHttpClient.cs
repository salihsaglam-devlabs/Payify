
using LinkPara.ApiGateway.Services.KKB.Models.Request;
using LinkPara.ApiGateway.Services.KKB.Models.Response;
using LinkPara.HttpProviders.KKB.Models;

namespace LinkPara.ApiGateway.Services.KKB.HttpClients
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

        public async Task<InquireIbanResponse> IbanInquireAsync(InquireIbanRequest request)
        {
            var response = await PostAsJsonAsync($"v1/KKB/inquire-iban", request);
            var result = await response.Content.ReadFromJsonAsync<InquireIbanResponse>();
            return result ?? throw new InvalidOperationException();
        }
    }
}
