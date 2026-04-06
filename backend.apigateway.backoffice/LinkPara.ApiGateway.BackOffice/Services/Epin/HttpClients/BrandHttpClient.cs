using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class BrandHttpClient : HttpClientBase, IBrandHttpClient
{
    public BrandHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<ActionResult<PaginatedList<BrandDto>>> GetFilterBrandsAsync(GetFilterBrandsRequest request)
    {
        var url = CreateUrlWithParams($"v1/Brands", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
    }
}
