using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;
public class UserPictureHttpClient : HttpClientBase, IUserPictureHttpClient
{
    public UserPictureHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<UserPictureDto> GetUserPictureAsync(string userId)
    {
        var response = await GetAsync($"v1/UserPictures?UserId={userId}");
        var userPicture = await response.Content.ReadFromJsonAsync<UserPictureDto>();
        return userPicture ?? throw new InvalidOperationException();
    }

    public async Task PostUserPictureAsync(UserPictureDto userPicture)
    {
        var response = await PostAsJsonAsync($"v1/UserPictures", userPicture);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
