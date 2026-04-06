using System.Net.Http.Json;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.Identity;

public class IdentityParametersService : HttpClientBase, IIdentityParametersService
{
    public IdentityParametersService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<List<string>> GetNotificationParametersWithDisplayNamesAsync(string language)
    {
        var response = await GetAsync($"v1/NotificationTemplateParameters/{language}");

        var parameters = await response.Content.ReadFromJsonAsync<List<string>>();

        return parameters ?? throw new InvalidOperationException();
    }

    public async Task<IdentityCustomNotificationParameters> GetNotificationParameterValuesAsync(GetIdentityParametersRequest request)
    {
        var response = await GetAsync($"v1/NotificationTemplateParameters/values/{request.Language}/{request.UserId}");

        var parameters = await response.Content.ReadFromJsonAsync<IdentityCustomNotificationParameters>();

        return parameters ?? throw new InvalidOperationException();
    }
}