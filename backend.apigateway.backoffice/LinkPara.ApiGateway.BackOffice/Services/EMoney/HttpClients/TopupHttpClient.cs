
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class TopupHttpClient : HttpClientBase, ITopupHttpClient
{

    public TopupHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<TopupCancelResponse> TopupCancelAsync(TopupCancelRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Topups/cancel", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var topupCancel = JsonSerializer.Deserialize<TopupCancelResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return topupCancel;
    }

    public async Task<PaginatedList<TopupResponse>> GetListAsync(GetTopupListRequest request)
    {
        var url = CreateUrlWithParams($"v1/Topups", request, true);
        var response = await GetAsync(url);
        var topups = await response.Content.ReadFromJsonAsync<PaginatedList<TopupResponse>>();
        return topups ?? throw new InvalidOperationException();
    }

    public async Task TopupReturnToWalletAsync(TopupReturnToWalletRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Topups/return-to-wallet", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

    }

    public async Task TopupUpdateStatusAsync(TopupUpdateStatusRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Topups/update-status", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

    }
}
