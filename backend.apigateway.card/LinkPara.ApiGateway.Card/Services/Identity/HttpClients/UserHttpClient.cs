using LinkPara.ApiGateway.Card.Commons.Extensions;
using LinkPara.ApiGateway.Card.Commons.Helpers;
using LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Card.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Card.Services.Identity.HttpClients;

public class UserHttpClient : HttpClientBase, IUserHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public UserHttpClient(HttpClient client,
        IServiceRequestConverter serviceRequestConverter,
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)

    {
        _serviceRequestConverter = serviceRequestConverter;
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
}