using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LinkPara.HttpProviders.Emoney;

public class ProvisionService : HttpClientBase, IProvisionService
{

    public ProvisionService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<ProvisionResponse> CancelProvisionAsync(ProvisionCancelRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Provisions", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackRequest request)
    {
        var response = await PutAsJsonAsync("v1/Provisions/cancel-cashback", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionCashbackResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<ProvisionResponse> ProvisionAsync(ProvisionRequest request)
    {
        var response = await PostAsJsonAsync("v1/Provisions", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackRequest request)
    {
        var response = await PostAsJsonAsync("v1/Provisions/cashback", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionCashbackResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<ProvisionPreviewResponse> ProvisionPreviewAsync(ProvisionPreviewRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Provisions", request);
        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionPreviewResponse = await response.Content.ReadFromJsonAsync<ProvisionPreviewResponse>();

        return provisionPreviewResponse ?? throw new InvalidOperationException();
    }

    public async Task<ProvisionResponse> ReturnProvisionAsync(ProvisionReturnRequest request)
    {
        var response = await PostAsJsonAsync("v1/Provisions/partially-return", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Provisions/inquire", request);
        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var inquireProvisionResponse = await response.Content.ReadFromJsonAsync<InquireProvisionResponse>();

        return inquireProvisionResponse ?? throw new InvalidOperationException();
    }
}
