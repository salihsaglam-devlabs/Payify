
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class MasterpassHttpClient : HttpClientBase, IMasterpassHttpClient
{
    public MasterpassHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<TopupCancelResponse> TopupCancelAsync(MasterpassCancelRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Masterpass/cancel", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var topupCancel = JsonSerializer.Deserialize<TopupCancelResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return topupCancel;
    }
}
