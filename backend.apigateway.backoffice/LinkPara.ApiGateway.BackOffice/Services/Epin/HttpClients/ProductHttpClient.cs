using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class ProductHttpClient : HttpClientBase, IProductHttpClient
{
    public ProductHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<ActionResult<List<ProductDto>>> GetFilterProductsAsync(GetFilterProductsRequest request)
    {
        var url = CreateUrlWithParams($"v1/Products", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
    }
}
