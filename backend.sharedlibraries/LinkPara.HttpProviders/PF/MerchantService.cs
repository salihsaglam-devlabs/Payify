using LinkPara.HttpProviders.PF.Models.Request;
using Microsoft.AspNetCore.Http;


namespace LinkPara.HttpProviders.PF
{
    public class MerchantService : HttpClientBase, IMerchantService
    {
        public MerchantService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task UpdateMerchantIKSAsync(UpdateMerchantIKSModel request)
        {
            var response = await PutAsJsonAsync("v1/Merchants/updateMerchantIKS", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
