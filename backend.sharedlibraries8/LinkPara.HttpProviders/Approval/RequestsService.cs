
using System.Net.Http.Json;
using LinkPara.HttpProviders.Approval.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.HttpProviders.Approval;

public class RequestsService : HttpClientBase, IRequestsService
{
    public RequestsService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequestsAsync([FromQuery] GetCashbackRequestsQuery query)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Requests/cashback", query, true);

        var response = await GetAsync(url);

        var requests = await response.Content.ReadFromJsonAsync<PaginatedList<RequestCashbackDto>>();

        return requests ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<RequestWalletBlockageDto>> GetAllWalletBlocakgeRequestsAsync([FromQuery] GetWalletBlockageRequestsQuery query)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Requests/wallet-blockage", query, true);

        var response = await GetAsync(url);

        var requests = await response.Content.ReadFromJsonAsync<PaginatedList<RequestWalletBlockageDto>>();

        return requests ?? throw new InvalidOperationException();
    }
}
