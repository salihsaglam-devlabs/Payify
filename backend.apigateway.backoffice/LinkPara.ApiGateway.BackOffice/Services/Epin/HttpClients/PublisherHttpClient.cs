using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class PublisherHttpClient : HttpClientBase, IPublisherHttpClient
{
    public PublisherHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<ActionResult<PaginatedList<PublisherDto>>> GetFilterPublishersAsync(GetFilterPublishersRequest request)
    {
        var url = CreateUrlWithParams($"v1/Publishers", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<PublisherDto>>();
    }
}
