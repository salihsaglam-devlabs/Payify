using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class LimitHttpClient : HttpClientBase, ILimitHttpClient
{
    public LimitHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task<AccountLimitDto> GetAccountLimitsRequestAsync(GetAccountLimitsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Limits/account" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var accountLimit = JsonSerializer.Deserialize<AccountLimitDto>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return accountLimit ?? throw new InvalidOperationException();
    }
}