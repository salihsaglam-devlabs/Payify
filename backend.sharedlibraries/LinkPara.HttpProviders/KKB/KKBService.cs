using LinkPara.HttpProviders.KKB.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.KKB
{
    public class KKBService : HttpClientBase, IKKBService
    {
        public KKBService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }
        public async Task<ValidateIbanResponse> ValidateIban(ValidateIbanRequest request)
        {
            var response = await PostAsJsonAsync("v1/Kkb/validate-iban", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var validateIbanResponse = await response.Content.ReadFromJsonAsync<ValidateIbanResponse>();

            return validateIbanResponse ?? throw new InvalidCastException();
        }
    }
}

