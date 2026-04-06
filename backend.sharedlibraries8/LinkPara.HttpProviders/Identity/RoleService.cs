using LinkPara.HttpProviders.Identity.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LinkPara.HttpProviders.Identity;

public class RoleService : HttpClientBase, IRoleService
{
    public RoleService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<RoleDetailDto> GetRoleAsync(Guid roleId)
    {
        var response = await GetAsync($"v1/Roles/{roleId}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var role = JsonSerializer.Deserialize<RoleDetailDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return role;
        }
        throw new InvalidOperationException();
    }

    public async Task<List<UserDto>> GetUsersByRoleIdAsync(Guid roleId)
    {
        var response = await GetAsync($"v1/Roles/{roleId}/users");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<UserDto>>(responseString,  new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return users;
        }
        throw new InvalidOperationException();
    }
}
