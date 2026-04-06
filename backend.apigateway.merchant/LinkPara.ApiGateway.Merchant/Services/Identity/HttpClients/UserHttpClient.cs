using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;
using LinkPara.ApiGateway.Merchant.Commons.Extensions;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

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

        return users ?? throw new InvalidOperationException();
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

    public async Task<UserCreateResponse> CreateUserAsync(CreateUserWithUserName request)
    {
        var response = await PostAsJsonAsync("v1/Users", request);

        var user = await response.Content.ReadFromJsonAsync<UserCreateResponse>();

        return user ?? throw new InvalidOperationException();
    }

    public async Task UpdateUserAsync(UpdateUserWithUserName request)
    {
        var response = await PutAsJsonAsync($"v1/Users", request);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<GetUserLoginActivityResponse> GetUserLoginActivity(Guid userId)
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
    
    public async Task<List<UserAgreementDocumentsStatusDto>> GetUserDocumentsAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}/agreementDocuments");

        var responseString = await response.Content.ReadAsStringAsync();

        var agreementDocument = JsonSerializer.Deserialize<List<UserAgreementDocumentsStatusDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return agreementDocument ?? throw new InvalidOperationException();
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Users/VerifyEmail", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
        var content = await response.Content.ReadAsStringAsync();
        return Boolean.Parse(content);
    }
}