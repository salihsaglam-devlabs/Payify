using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;

public class RolesHttpClient : HttpClientBase, IRolesHttpClient
{
    public RolesHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<RoleDto>> GetBoaRolesAsync(RoleFilterQuery request)
    {
        var url = CreateUrlWithParams($"v1/BoaRoles", request, true);
        var response = await GetAsync(url);
        var roles = await response.Content.ReadFromJsonAsync<PaginatedList<RoleDto>>();
        return roles ?? throw new InvalidOperationException();
    }
}