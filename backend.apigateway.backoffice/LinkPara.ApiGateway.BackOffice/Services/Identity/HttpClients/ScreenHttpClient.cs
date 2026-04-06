using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public class ScreenHttpClient : HttpClientBase, IScreenHttpClient
    {
        public ScreenHttpClient(HttpClient client,IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {
        }

        public async Task UpdateRoleScreenAsync(UpdateRoleScreenRequest request)
        {
            await PutAsJsonAsync($"v1/Screens/{request.RoleId}", request);
        }

        public async Task<List<ScreenDto>> GetAllScreensAsync(GetRoleScreensRequest request)
        {
            var response = await GetAsync($"v1/Screens");

            var responseString = await response.Content.ReadAsStringAsync();

            var screens = JsonSerializer.Deserialize<List<ScreenDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return screens ?? throw new InvalidOperationException();
        }

        public async Task<List<ScreenDto>> GetUserRoleScreenAsync(string userId)
        {
            var response = await GetAsync($"v1/Screens/role-menu/{userId}");

            var responseString = await response.Content.ReadAsStringAsync();

            var screens = JsonSerializer.Deserialize<List<ScreenDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            return screens ?? throw new InvalidOperationException();
        }
        public async Task<RoleScreenDto> GetRoleScreenAsync(string roleId)
        {
            var response = await GetAsync($"v1/Screens/{roleId}");

            var responseString = await response.Content.ReadAsStringAsync();

            var screens = JsonSerializer.Deserialize<RoleScreenDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            return screens ?? throw new InvalidOperationException();
        }
        public async Task CreateRoleScreenAsync(RoleScreenRequest request)
        {
            await PostAsJsonAsync("v1/Screens", request);
        }

        public async Task DeleteRoleScreenAsync(string roleId)
        {
            await DeleteAsync($"v1/Screens/{roleId}");
        }
    }
}
