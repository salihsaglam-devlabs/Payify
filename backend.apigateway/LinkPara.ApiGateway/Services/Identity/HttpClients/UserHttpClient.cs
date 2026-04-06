using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public class UserHttpClient : HttpClientBase, IUserHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public UserHttpClient(HttpClient client,
        IServiceRequestConverter serviceRequestConverter,
        IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var response = await GetAsync($"v1/Users/{userId}");
        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user ?? throw new InvalidOperationException();
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
    public async Task<bool> GetIsUserExistAsync(string userName)
    {
        var response = await GetAsync($"v1/Users/user-exist/{userName}");
        var responseString = await response.Content.ReadAsStringAsync();

        var isExist = JsonSerializer.Deserialize<bool>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return isExist;
    }

    public async Task<UserDto> GetUserAsync(GetUserRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetUserRequest, GetUserServiceRequest>(request);

        var response = await GetAsync($"v1/Users/{clientRequest.UserId}");
        var responseString = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (user is not null)
        {
            user.Id = clientRequest.UserId;
        }

        return user ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<UserDto>> GetUsersAsync(UserFilterRequest filter)
    {
        var queryString = filter.GetQueryString();

        var response = await GetAsync($"v1/Users/all" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        var userList = JsonSerializer.Deserialize<PaginatedList<UserDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userList ?? throw new InvalidOperationException();
    }

    public async Task UpdateUserAsync(UpdateUserRequest user)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateUserRequest, UpdateUserServiceRequest>(user);

        await PutAsJsonAsync($"v1/Users/{clientRequest.UserId}", clientRequest);
    }

    public async Task UpdateKycAsync(UpdateKycRequest user)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateKycRequest, UpdateKycServiceRequest>(user);

        await PutAsJsonAsync($"v1/Users/{clientRequest.UserId}/kyc", clientRequest);
    }

    public async Task<List<UserAgreementDocumentsStatusDto>> GetUserDocumentsAsync(Guid userId, UserDocumentFilterRequest request)
    {
        var query = request.GetQueryString();

        var response = await GetAsync($"v1/Users/{userId}/agreementDocuments" + query);

        var responseString = await response.Content.ReadAsStringAsync();

        var agreementDocument = JsonSerializer.Deserialize<List<UserAgreementDocumentsStatusDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return agreementDocument ?? throw new InvalidOperationException();
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

    public async Task<UserIdByUserNameResponse> GetUserIdByUserNameAsync(string userName)
    {
        var response = await GetAsync($"v1/Users/get-user-id-by-username/{userName}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
        var responseString = await response.Content.ReadAsStringAsync();

        var userId = JsonSerializer.Deserialize<UserIdByUserNameResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userId;
    }
}