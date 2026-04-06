using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Response;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class UserSimBlockageHttpClient : HttpClientBase, IUserSimBlockageHttpClient
{
    public UserSimBlockageHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<UserSimBlockageDto>> GetUserSimBlockageListAsync(GetUserSimBlockageRequest request)
    {
        var url = CreateUrlWithParams($"v1/UserSimBlockage", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var userSimBlockageList = await response.Content.ReadFromJsonAsync<PaginatedList<UserSimBlockageDto>>();

        return userSimBlockageList ?? throw new InvalidOperationException();
    }

    public async Task RemoveUserSimBlockageAsync(RemoveUserSimBlockageRequest request)
    {
        var response = await PutAsJsonAsync($"v1/UserSimBlockage", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
