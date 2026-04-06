using System.Net.Http.Json;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.PF;

public class PfParametersService : HttpClientBase, IPfParametersService
{
    public PfParametersService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<List<string>> GetNotificationParametersWithDisplayNamesAsync(string language)
    {
        var response = await GetAsync($"v1/NotificationTemplateParameters/{language}");

        var parameters = await response.Content.ReadFromJsonAsync<List<string>>();

        return parameters ?? throw new InvalidOperationException();
    }

    public async Task<PfCustomNotificationParameters> GetNotificationParameterValuesAsync(GetPfParameterValuesRequest request)
    {
        var response = await GetAsync($"v1/NotificationTemplateParameters/values/{request.Language}/{request.MerchantId}");

        var parameters = await response.Content.ReadFromJsonAsync<PfCustomNotificationParameters>();

        return parameters ?? throw new InvalidOperationException();
    }
}