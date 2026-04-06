using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public class UserInboxHttpClient : HttpClientBase, IUserInboxHttpClient
    {
        public UserInboxHttpClient(HttpClient client,
            IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }
        public async Task<List<UserInboxDto>> GetUserInboxAsync(UserInboxRequest filter)
        {
            var queryString = filter.GetQueryString();
            var response = await GetAsync($"v1/UserInbox"+queryString);
            var responseString = await response.Content.ReadAsStringAsync();
            var userInboxList = JsonSerializer.Deserialize<List<UserInboxDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userInboxList ?? throw new InvalidOperationException();
        }

        public async Task UpdateReadedUserInboxAsync(UserInboxRequest request)
        {
            await PutAsJsonAsync($"v1/UserInbox/read-all", request);
        }

        public async Task DeleteSelectedAsync(DeleteUserInboxRequest ids)
        {
            var content = new StringContent(JsonSerializer.Serialize(ids), Encoding.UTF8, "application/json");
            await PatchAsync("v1/UserInbox/delete-selected", content);
        }
    }
}
