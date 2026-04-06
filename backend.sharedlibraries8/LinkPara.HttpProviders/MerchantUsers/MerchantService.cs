using LinkPara.HttpProviders.MerchantUsers.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.MerchantUsers;

public class MerchantService : HttpClientBase, IMerchantService
{

    public MerchantService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<GetMerchantUserResponse> GetMerchantUser(Guid userId)
    {
        var response = await GetAsync($"v1/Merchants/{userId}/Users");

        var merchantUserResponse = await response.Content.ReadFromJsonAsync<GetMerchantUserResponse>();

        return merchantUserResponse ?? throw new InvalidOperationException();
    }
}
