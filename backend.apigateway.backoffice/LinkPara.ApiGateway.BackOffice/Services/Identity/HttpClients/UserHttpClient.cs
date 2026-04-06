using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using System.Web;
using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public class UserHttpClient : HttpClientBase, IUserHttpClient
{
    public UserHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<UserDtoWithRoles>> GetAllUsersAsync(GetUsersRequest request)
    {
        var url = CreateUrlWithParams($"v1/Users", request, true);

        var response = await GetAsync(url);

        var users = await response.Content.ReadFromJsonAsync<PaginatedList<UserDtoWithRoles>>();

        if (!CanSeeSensitiveData())
        {
            users.Items.ForEach(s =>
            {
                s.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", s.FirstName);
                s.LastName = SensitiveDataHelper.MaskSensitiveData("Name", s.LastName);
                s.Email = SensitiveDataHelper.MaskSensitiveData("Email", s.Email);
                s.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.PhoneNumber);
                s.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", s.IdentityNumber);
            });
        }

        return users ?? throw new InvalidOperationException();
    }

    public async Task<ExistingUsersDto> GetExistingUserListAsync(GetExistingUsersRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/Users/check-existing-users" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        var userList = JsonSerializer.Deserialize<ExistingUsersDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userList ?? throw new InvalidOperationException();
    }

    public async Task<UserDetailDto> GetUserDetailsAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}");

        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDetailDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (!CanSeeSensitiveData())
        {
            user.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", user.FirstName);
            user.LastName = SensitiveDataHelper.MaskSensitiveData("Name", user.LastName);
            user.Email = SensitiveDataHelper.MaskSensitiveData("Email", user.Email);
            user.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", user.PhoneNumber);
            user.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", user.IdentityNumber);

        }

        return user ?? throw new InvalidOperationException();
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}");

        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user ?? throw new InvalidOperationException();
    }

    public async Task<List<ClaimDto>> GetUserClaimsAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}/claims");

        var responseString = await response.Content.ReadAsStringAsync();

        var userClaims = JsonSerializer.Deserialize<List<ClaimDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userClaims ?? throw new InvalidOperationException();
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}/roles");

        var responseString = await response.Content.ReadAsStringAsync();

        var userRole = JsonSerializer.Deserialize<List<RoleDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userRole ?? throw new InvalidOperationException();
    }

    public async Task AssignUserRoleAsync(Guid userId, UserRoleDto request)
    {
        var response = await PostAsJsonAsync($"v1/Users/{userId}/roles", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task<UserDto> GetUserByUserNameAsync(string userName)
    {
        var response = await GetAsync($"v1/Users/username/{userName}");
        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user ?? throw new InvalidOperationException();
    }
    public async Task<UserDto> PatchUserAsync(Guid userId, JsonPatchDocument<PatchUserRequest> request)
    {
        var response = await PatchAsync($"v1/Users/{userId}", request);

        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user ?? throw new InvalidOperationException();
    }

    public async Task<UserCreateResponse> CreateUserAsync(CreateUserWithUserName request)
    {
        var response = await PostAsJsonAsync("v1/Users", request);

        var user = await response.Content.ReadFromJsonAsync<UserCreateResponse>();

        return user ?? throw new InvalidOperationException();
    }

    public async Task<TokenResponse> GetUserTokenAsync(Guid userId, string secureKey)
    {
        var response = await GetAsync($"v1/Users/{userId}/token?secureKey={HttpUtility.UrlEncode(secureKey)}");

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public async Task UpdateUserAsync(UpdateUserWithUserName request)
    {
        var response = await PutAsJsonAsync($"v1/Users", request);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<GetUserLoginActivityResponse> GetUserLoginActivityAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}/loginActivity");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var responseString = await response.Content.ReadAsStringAsync();

        var loginActivity = JsonSerializer.Deserialize<GetUserLoginActivityResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return loginActivity;
    }

    public async Task RemoveUserLockAsync(Guid userId)
    {
        var response = await PutAsJsonAsync($"v1/Users/{userId}/removeUserLock", "");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }
  
    public async Task<bool> ResendEmailVerifyAsync(ResendEmailVerifyRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Users/ResendEmailVerify", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
        var content= await response.Content.ReadAsStringAsync();
        return bool.Parse(content);
    }

    public async Task ResetUserSecurityPictureAsync(Guid userId)
    {
        await DeleteAsync($"v1/SecurityPictures/{userId}/picture");
    }
}