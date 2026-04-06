using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public class AdvancedNotificationTemplatesHttpClient : HttpClientBase, IAdvancedTemplatesHttpClient
{
    public AdvancedNotificationTemplatesHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task CreateAdvancedTemplateAsync(CreateAdvancedTemplateRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Templates", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAdvancedTemplateAsync(UpdateAdvancedTemplateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Templates", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<TemplateDto>> GetAllAsync(GetTemplatesFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Templates", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationTemplateList = await response.Content.ReadFromJsonAsync<PaginatedList<TemplateDto>>();

        return notificationTemplateList ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<TemplateContentDto>> GetFilterContentsAsync(GetFilterContentsRequest request)
    {
        var url = CreateUrlWithParams($"v1/Templates/contents", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var contentList = await response.Content.ReadFromJsonAsync<PaginatedList<TemplateContentDto>>();

        return contentList ?? throw new InvalidOperationException();
    }

    public async Task<DefaultTemplateDto> GetDefaultTemplate(GetDefaultTemplateRequest request)
    {
        var url = CreateUrlWithParams($"v1/Templates/default", request, true);

        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var defaultTemplate = await response.Content.ReadFromJsonAsync<DefaultTemplateDto>();

        return defaultTemplate ?? throw new InvalidOperationException();
    }

    public async Task DeleteTemplateById(Guid templateId)
    {
        var response = await DeleteAsync($"v1/Templates/{templateId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task<TemplateDto> GetTemplateById(Guid templateId)
    {
        var response = await GetAsync($"v1/Templates/{templateId}");

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

        var notificationTemplateList = await response.Content.ReadFromJsonAsync<TemplateDto>();

        return notificationTemplateList ?? throw new InvalidOperationException();
    }
}