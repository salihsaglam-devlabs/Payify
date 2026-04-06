using LinkPara.HttpProviders.KPS.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkPara.HttpProviders.KPS
{
    public class KpsService : HttpClientBase, IKpsService
    {
        public KpsService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task<KpsResponse> GetPersonalInformation(KpsServiceRequest request)
        {
            var url = GetQueryString.CreateUrlWithParams("v1/KPS/identity-information", request);

            var response = await GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<KpsResponse>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<ValidateIdentityResponse> ValidateIdentity(ValidateIdentityRequest request)
        {
            var response = await PostAsJsonAsync("v1/KPS/validate-identity", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var validateIdentityResponse = await response.Content.ReadFromJsonAsync<ValidateIdentityResponse>();

            return validateIdentityResponse;
        }
    }
}
