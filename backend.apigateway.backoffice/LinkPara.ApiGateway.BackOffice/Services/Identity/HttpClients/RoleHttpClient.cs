using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json;
using LinkPara.SharedModels.Pagination;
using LinkPara.HttpProviders.Utility;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public class RoleHttpClient : HttpClientBase, IRoleHttpClient
{
    public RoleHttpClient(HttpClient client, 
        IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task CreateRoleAsync(RoleRequest request)
    {
        await PostAsJsonAsync("v1/Roles", request);
    }

    public async Task UpdateRoleAsync(UpdateRoleRequest request)
    {
        await PutAsJsonAsync($"v1/Roles/{request.RoleId}", request);
    }

    public async Task DeleteRoleAsync(string roleId)
    {
        await DeleteAsync($"v1/Roles/{roleId}");
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

    public async Task<RoleDetailDto> GetRoleAsync(string roleId)
    {
        var response = await GetAsync($"v1/Roles/{roleId}");

        var responseString = await response.Content.ReadAsStringAsync();

        var role = JsonSerializer.Deserialize<RoleDetailDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return role ?? throw new InvalidOperationException();
    }

    public async Task<List<UserDto>> GetUsersByRoleIdAsync(Guid roleId)
    {
        var response = await GetAsync($"v1/Roles/{roleId}/users");

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        return users ?? throw new InvalidOperationException();
    }
}