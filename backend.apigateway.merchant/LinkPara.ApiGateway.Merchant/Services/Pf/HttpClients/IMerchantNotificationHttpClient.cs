using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantNotificationHttpClient
{
    Task<MerchantNotificationTemplateDto> GetMerchantNotificationTemplateAsync(GetMerchantNotificationTemplateRequest request);
}