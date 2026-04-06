using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkPara.HttpProviders.Identity;

public class UserService : HttpClientBase, IUserService
{
    public UserService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        throw new InvalidOperationException();
    }

    public async Task<PaginatedList<UserDto>> GetAllUsersAsync(GetUsersRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Users/all", request, true);

        var response = await GetAsync(url);

        var users = await response.Content.ReadFromJsonAsync<PaginatedList<UserDto>>();

        return users ?? throw new InvalidOperationException();
    }

    public async Task<List<UserDto>> GetApplicationUserAsync(GetAppUsersRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Users/app-user", request);

        var response = await GetAsync(url);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        return users ?? throw new InvalidOperationException();
    }

    public async Task<UserCreateResponse> CreateUserAsync(CreateUserRequest request)
    {
        var response = await PostAsJsonAsync("v1/Users", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var userId = await response.Content.ReadFromJsonAsync<UserCreateResponse>();

        return userId ?? throw new InvalidOperationException();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await PostAsJsonAsync("v1/Auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var token = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return token ?? throw new InvalidOperationException();
    }

    public async Task<List<UserDeviceInfoDto>> GetUserDeviceInfo(GetUserDeviceInfoRequest request)
    {
        var response = await PostAsJsonAsync("v1/DeviceInfo/user-device", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var deviceList = await response.Content.ReadFromJsonAsync<List<UserDeviceInfoDto>>();

        return deviceList ?? throw new InvalidOperationException();
    }

    public async Task<UserDto> PatchAsync(Guid userId, JsonPatchDocument<PatchUserRequest> request)
    {
        var response = await PatchAsync($"v1/Users/{userId}", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task CreateUserAnswerAsync(CreateUserAnswerRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Questions/{request.SecurityQuestionId}/answer", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task UpdateUserAsync(UpdateUserWithUserName request)
    {
        var response = await PutAsJsonAsync($"v1/Users", request);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<GetUserLoginActivityResponse> GetUserLoginActivityAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}/loginActivity");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var userLoginActivity = JsonSerializer.Deserialize<GetUserLoginActivityResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userLoginActivity;
        }
        throw new InvalidOperationException();
    }

    public async Task<GetUserLoginActivityResponse> GetUserLoginActivityByChannelAsync(Guid userId, string channel)
    {
        var response = await GetAsync($"v1/Users/{userId}/loginActivity/{channel}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var userLoginActivity = JsonSerializer.Deserialize<GetUserLoginActivityResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userLoginActivity;
        }
        throw new InvalidOperationException();
    }

    public async Task<List<UserDto>> GetUsersByIdsAsync(GetUsersByIdsRequest request)
    {
        var response = await PostAsJsonAsync("v1/Users/search-by-ids", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        return users ?? throw new InvalidOperationException();
    }
}

