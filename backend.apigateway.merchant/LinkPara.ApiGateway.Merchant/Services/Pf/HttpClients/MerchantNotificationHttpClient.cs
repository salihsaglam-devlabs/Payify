using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class MerchantNotificationHttpClient : HttpClientBase, IMerchantNotificationHttpClient
{
    public MerchantNotificationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<MerchantNotificationTemplateDto> GetMerchantNotificationTemplateAsync(GetMerchantNotificationTemplateRequest request)
    {
        var response = await GetAsync($"v1/MerchantNotification?Id={request.Id}&ContentSource={request.ContentSource}&Language={request.Language}");
        var notificationTemplate = await response.Content.ReadFromJsonAsync<MerchantNotificationTemplateDto>();
        return notificationTemplate ?? throw new InvalidOperationException();
    }
}