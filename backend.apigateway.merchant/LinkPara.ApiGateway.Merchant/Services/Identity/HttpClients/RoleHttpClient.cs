using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public class RoleHttpClient : HttpClientBase, IRoleHttpClient
{
    public RoleHttpClient(HttpClient client, 
        IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<RoleDto>> GetAllRolesAsync(GetRolesRequest request) 
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Roles", request,true);
        
        var response = await GetAsync(url);

        var responseString = await response.Content.ReadAsStringAsync();

        var roles = JsonSerializer.Deserialize<PaginatedList<RoleDto>>(responseString, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        return roles ?? throw new InvalidOperationException();
    }
}